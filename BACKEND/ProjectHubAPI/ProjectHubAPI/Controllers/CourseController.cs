using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.Models;
using ProjectHubAPI.DTOs;
using MapsterMapper;

namespace ProjectHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public CourseController(AppDbContext context, IWebHostEnvironment env, IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var modules = await _context.CourseModules
                .Where(m => m.CourseId == id)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            var courseDto = _mapper.Map<CourseDto>(course);
            courseDto.Modules = _mapper.Map<List<CourseModuleDto>>(modules);

            return courseDto;
        }

        [HttpPost("{courseId}/modules")]
        public async Task<IActionResult> AddModule(int courseId, CreateCourseModuleDto dto)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var module = _mapper.Map<CourseModule>(dto);
            module.CourseId = courseId;

            _context.CourseModules.Add(module);
            await _context.SaveChangesAsync();
            return Ok(module);
        }

        [HttpPost("complete-module/{enrollmentId}/{moduleId}")]
        public async Task<IActionResult> CompleteModule(int enrollmentId, int moduleId)
        {
            var enrollment = await _context.Enrollments.Include(e => e.Course).FirstOrDefaultAsync(e => e.Id == enrollmentId);
            if (enrollment == null) return NotFound("Enrollment not found");

            var module = await _context.CourseModules.FindAsync(moduleId);
            if (module == null || module.CourseId != enrollment.CourseId) return BadRequest("Invalid module");

            var completion = await _context.ModuleCompletions
                .FirstOrDefaultAsync(mc => mc.EnrollmentId == enrollmentId && mc.ModuleId == moduleId);

            if (completion == null)
            {
                completion = new ModuleCompletion
                {
                    EnrollmentId = enrollmentId,
                    ModuleId = moduleId,
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow
                };
                _context.ModuleCompletions.Add(completion);
            }
            else
            {
                completion.IsCompleted = true;
                completion.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var totalModules = await _context.CourseModules.CountAsync(m => m.CourseId == enrollment.CourseId);
            var completedModules = await _context.ModuleCompletions.CountAsync(mc => mc.EnrollmentId == enrollmentId && mc.IsCompleted);

            if (totalModules > 0)
            {
                enrollment.ProgressPercentage = (completedModules * 100) / totalModules;
                if (enrollment.ProgressPercentage >= 100 && !enrollment.IsCompleted)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletionDate = DateTime.UtcNow;
                    enrollment.Status = "Completed";
                }
                else if (enrollment.ProgressPercentage > 0)
                {
                    enrollment.Status = "In Progress";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { progress = enrollment.ProgressPercentage, status = enrollment.Status });
        }

        [HttpPost("submit-quiz/{enrollmentId}")]
        public async Task<IActionResult> SubmitQuiz(int enrollmentId, [FromBody] int score)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound("Enrollment not found");

            enrollment.QuizScore = score;
            await _context.SaveChangesAsync();

            return Ok(new { score = enrollment.QuizScore });
        }

        [HttpPost("update-progress/{enrollmentId}")]
        public async Task<IActionResult> UpdateProgress(int enrollmentId, [FromBody] int progress)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound("Enrollment not found");

            enrollment.ProgressPercentage = progress;
            if (progress >= 100 && !enrollment.IsCompleted)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletionDate = DateTime.UtcNow;
                enrollment.Status = "Completed";
            }
            
            await _context.SaveChangesAsync();
            return Ok(enrollment);
        }

        [HttpGet("team-stats")]
        public async Task<ActionResult<object>> GetTeamStats()
        {
            var totalEnrollments = await _context.Enrollments.CountAsync();
            if (totalEnrollments == 0) return Ok(new { totalEnrollments = 0, completionRate = 0 });

            var completedEnrollments = await _context.Enrollments.CountAsync(e => e.IsCompleted);
            var completionRate = Math.Round((double)completedEnrollments / totalEnrollments * 100);

            var popularCourses = await _context.Enrollments
                .Include(e => e.Course)
                .GroupBy(e => e.CourseId)
                .Select(g => new { 
                    CourseId = g.Key, 
                    Title = g.First().Course.Title, 
                    EnrollmentCount = g.Count() 
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .Take(5)
                .ToListAsync();

            var topLearners = await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.IsCompleted)
                .GroupBy(e => e.UserId)
                .Select(g => new {
                    UserId = g.Key,
                    Name = g.First().User.Name,
                    Completions = g.Count(),
                    AvgScore = Math.Round(g.Average(e => e.QuizScore))
                })
                .OrderByDescending(x => x.Completions)
                .ThenByDescending(x => x.AvgScore)
                .Take(5)
                .ToListAsync();

            return Ok(new {
                TotalEnrollments = totalEnrollments,
                CompletedEnrollments = completedEnrollments,
                CompletionRate = completionRate,
                PopularCourses = popularCourses,
                TopLearners = topLearners
            });
        }

        [HttpGet("team-achievements")]
        public async Task<ActionResult<IEnumerable<object>>> GetTeamAchievements()
        {
            var achievements = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.IsCompleted && e.QuizScore >= 70)
                .OrderByDescending(e => e.CompletionDate)
                .Select(e => new {
                    e.Id,
                    e.UserId,
                    UserName = e.User.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    e.CompletionDate,
                    e.QuizScore
                })
                .ToListAsync();

            return Ok(achievements);
        }

        [HttpGet("my-courses/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyCourses(int userId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            var result = new List<object>();
            foreach (var e in enrollments)
            {
                var totalModules = await _context.CourseModules.CountAsync(m => m.CourseId == e.CourseId);
                var completedModulesData = await _context.ModuleCompletions
                    .Where(mc => mc.EnrollmentId == e.Id && mc.IsCompleted)
                    .Select(mc => mc.ModuleId)
                    .ToListAsync();
                
                result.Add(new
                {
                    e.Id,
                    e.UserId,
                    e.CourseId,
                    CourseTitle = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    e.Status,
                    e.EnrolledDate,
                    e.ProgressPercentage,
                    e.IsCompleted,
                    e.CompletionDate,
                    e.QuizScore,
                    e.IsMandatory,
                    e.DueDate,
                    e.AssignedById,
                    TotalModules = totalModules,
                    CompletedModules = completedModulesData.Count,
                    CompletedModuleIds = completedModulesData
                });
            }
            return result;
        }

        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(int courseId, [FromBody] int userId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course not found");

            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);
            
            if (existingEnrollment != null) return BadRequest("Already enrolled");

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrolledDate = DateTime.UtcNow,
                Status = "Enrolled",
                ProgressPercentage = 0
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(enrollment);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignCourse([FromBody] AssignCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) return NotFound("Course not found");

            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == dto.CourseId && e.UserId == dto.UserId);
            
            if (existingEnrollment != null)
            {
                existingEnrollment.IsMandatory = true;
                existingEnrollment.AssignedById = dto.AssignedById;
                existingEnrollment.DueDate = dto.DueDate;
                
                var notification = new Notification
                {
                    UserId = dto.UserId,
                    Title = "Course Assignment Updated",
                    Message = $"Your assignment for '{course.Title}' has been updated.",
                    Type = "System",
                    RelatedId = dto.CourseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                return Ok(existingEnrollment);
            }

            var enrollment = new Enrollment
            {
                CourseId = dto.CourseId,
                UserId = dto.UserId,
                EnrolledDate = DateTime.UtcNow,
                Status = "Enrolled",
                ProgressPercentage = 0,
                IsMandatory = true,
                AssignedById = dto.AssignedById,
                DueDate = dto.DueDate
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(enrollment);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, Course course)
        {
            if (id != course.Id) return BadRequest();

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var modules = _context.CourseModules.Where(m => m.CourseId == id);
            _context.CourseModules.RemoveRange(modules);

            var enrollments = _context.Enrollments.Where(e => e.CourseId == id);
            _context.Enrollments.RemoveRange(enrollments);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        [HttpPost("seed-sample")]
        public async Task<ActionResult<Course>> SeedSampleCourse()
        {
            var quizDataJson = @"[
                {""question"": ""What is the capital of France?"", ""options"": [{""text"": ""London"", ""isCorrect"": false}, {""text"": ""Paris"", ""isCorrect"": true}, {""text"": ""Berlin"", ""isCorrect"": false}]},
                {""question"": ""What is 2 + 2?"", ""options"": [{""text"": ""3"", ""isCorrect"": false}, {""text"": ""4"", ""isCorrect"": true}, {""text"": ""5"", ""isCorrect"": false}]}
            ]";

            var course = new Course
            {
                Title = "Introduction to C# Video Masterclass",
                Description = "A complete guide to C# development featuring interactive video modules, quizzes, and a final certificate.",
                ThumbnailUrl = "https://images.unsplash.com/photo-1542831371-29b0f74f9713?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                Duration = "4h 30m",
                Category = "Development",
                CreatedAt = DateTime.UtcNow,
                QuizData = quizDataJson
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var modules = new List<CourseModule>
            {
                new CourseModule { CourseId = course.Id, Title = "Module 1: Introduction to C# Concepts", Content = @"<p>In this module, we will explore the syntax of C#.</p><iframe width=""560"" height=""315"" src=""https://www.youtube.com/embed/gfkTfcpWqAY"" frameborder=""0"" allow=""accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"" allowfullscreen></iframe>", OrderIndex = 1 },
                new CourseModule { CourseId = course.Id, Title = "Module 2: Object-Oriented Programming", Content = @"<p>Learn how to use classes and objects.</p><iframe width=""560"" height=""315"" src=""https://www.youtube.com/embed/ZzlrN2nI-uE"" frameborder=""0"" allow=""accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"" allowfullscreen></iframe>", OrderIndex = 2 },
                new CourseModule { CourseId = course.Id, Title = "Module 3: Advanced Topics", Content = @"<p>LINQ, Async/Await and more.</p><a href=""https://docs.microsoft.com/en-us/dotnet/csharp/"" target=""_blank"">Read Official Documentation</a>", OrderIndex = 3 }
            };

            _context.CourseModules.AddRange(modules);
            await _context.SaveChangesAsync();

            return Ok(course);
        }

        [HttpPost("{id}/upload-video")]
        public async Task<IActionResult> UploadCourseVideo(int id, IFormFile file)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "courses");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            course.VideoUrl = $"/uploads/courses/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { url = course.VideoUrl });
        }

        [HttpPost("modules/{moduleId}/upload-video")]
        public async Task<IActionResult> UploadModuleVideo(int moduleId, IFormFile file)
        {
            var module = await _context.CourseModules.FindAsync(moduleId);
            if (module == null) return NotFound();

            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "modules");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            
            module.Content = $"<video width=\"100%\" controls class=\"premium-video\"><source src=\"/uploads/modules/{fileName}\" type=\"video/mp4\">Your browser does not support the video tag.</video>";
            await _context.SaveChangesAsync();

            return Ok(new { url = $"/uploads/modules/{fileName}" });
        }
    }
}
 
