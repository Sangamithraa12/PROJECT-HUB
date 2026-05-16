using ProjectHubAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ProjectHubAPI.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            InitializeSchema(context);

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Manager" },
                    new Role { Name = "Employee" }
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");
                var employeeRole = context.Roles.First(r => r.Name == "Employee");
                var managerRole = context.Roles.First(r => r.Name == "Manager");

                context.Users.AddRange(
                    new User
                    {
                        Name = "System Admin",
                        Email = "admin@projecthub.com",
                        Password = "Admin123",
                        RoleId = adminRole.Id
                    },
                    new User
                    {
                        Name = "Madhu",
                        Email = "madhu@projecthub.com",
                        Password = "User123",
                        RoleId = employeeRole.Id
                    },
                    new User
                    {
                        Name = "Sangamithra",
                        Email = "sangamithra@projecthub.com",
                        Password = "Manager123",
                        RoleId = managerRole.Id
                    }
                );
                context.SaveChanges();
            }

            if (!context.Courses.Any())
            {
                context.Courses.AddRange(
                    new Course
                    {
                        Title = "Advanced Angular Masterclass",
                        Description = "Master Angular and build high-performance web applications with advanced features. Learn about RxJS, State Management, and Standalone Components.",
                        ThumbnailUrl = "https://images.pexels.com/photos/11035471/pexels-photo-11035471.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "12 Hours",
                        Category = "Development",
                        TargetRole = "Employee",
                        VideoUrl = "https://www.youtube.com/embed/k5E2AVpwsko",
                        QuizData = "[{\"question\":\"What is the primary benefit of Standalone Components?\",\"options\":[{\"text\":\"Reduced bundle size\",\"isCorrect\":false},{\"text\":\"Simplified dependencies without NgModules\",\"isCorrect\":true},{\"text\":\"Faster CSS loading\",\"isCorrect\":false},{\"text\":\"Automatic database syncing\",\"isCorrect\":false}]}]"
                    },
                    new Course
                    {
                        Title = "Modern .NET API Design",
                        Description = "Learn to build scalable, secure, and well-designed web APIs with latest .NET features like Minimal APIs and Entity Framework Core.",
                        ThumbnailUrl = "https://images.pexels.com/photos/546819/pexels-photo-546819.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "8 Hours",
                        Category = "Development",
                        TargetRole = "Employee",
                        VideoUrl = "https://www.youtube.com/embed/uK8f0_XIn4g",
                        QuizData = "[{\"question\":\"Which .NET feature allows for building lightweight APIs?\",\"options\":[{\"text\":\"WebForms\",\"isCorrect\":false},{\"text\":\"Minimal APIs\",\"isCorrect\":true},{\"text\":\"Silverlight\",\"isCorrect\":false},{\"text\":\"WPF\",\"isCorrect\":false}]}]"
                    },
                    new Course
                    {
                        Title = "UI/UX Principles for Developers",
                        Description = "Enhance the user experience of your apps with core design principles, typography, and effective layouts.",
                        ThumbnailUrl = "https://images.pexels.com/photos/196644/pexels-photo-196644.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "6 Hours",
                        Category = "Design",
                        TargetRole = "Employee",
                        VideoUrl = "https://www.youtube.com/embed/zHAa-m16NGk",
                        QuizData = "[{\"question\":\"What does UX stand for?\",\"options\":[{\"text\":\"User Xylophone\",\"isCorrect\":false},{\"text\":\"User Experience\",\"isCorrect\":true},{\"text\":\"Unit Exchange\",\"isCorrect\":false},{\"text\":\"Universal X-factor\",\"isCorrect\":false}]}]"
                    },
                    new Course
                    {
                        Title = "High-Performance Leadership",
                        Description = "Learn how to motivate teams, set effective OKRs, and build a culture of accountability. Essential for first-time and seasoned managers.",
                        ThumbnailUrl = "https://images.pexels.com/photos/3184291/pexels-photo-3184291.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "10 Hours",
                        Category = "Leadership",
                        TargetRole = "Manager",
                        VideoUrl = "https://www.youtube.com/embed/fW8amMCVAJQ",
                        QuizData = "[{\"question\":\"What are OKRs?\",\"options\":[{\"text\":\"Occasional Key Results\",\"isCorrect\":false},{\"text\":\"Objectives and Key Results\",\"isCorrect\":true},{\"text\":\"Online Key Resources\",\"isCorrect\":false},{\"text\":\"Optimal Knowledge Reports\",\"isCorrect\":false}]}]"
                    },
                    new Course
                    {
                        Title = "Conflict Resolution & Feedback",
                        Description = "Master the art of constructive feedback and learning to navigate difficult team dynamics with empathy and professionalism.",
                        ThumbnailUrl = "https://images.pexels.com/photos/3184339/pexels-photo-3184339.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "5 Hours",
                        Category = "Leadership",
                        TargetRole = "Manager",
                        VideoUrl = "https://www.youtube.com/embed/9_1K3vBtu8w",
                        QuizData = "[{\"question\":\"What is the 'Feedback Sandwich'?\",\"options\":[{\"text\":\"Bread, Meat, Bread\",\"isCorrect\":false},{\"text\":\"Positive, Constructive, Positive\",\"isCorrect\":true},{\"text\":\"Soft, Hard, Soft\",\"isCorrect\":false},{\"text\":\"Start, Stop, Continue\",\"isCorrect\":false}]}]"
                    },
                    new Course
                    {
                        Title = "Strategic Resource Planning",
                        Description = "Optimize your department's output by mastering budget management and human resource allocation strategies.",
                        ThumbnailUrl = "https://images.pexels.com/photos/1181311/pexels-photo-1181311.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
                        Duration = "7 Hours",
                        Category = "Management",
                        TargetRole = "Manager",
                        VideoUrl = "https://www.youtube.com/embed/mYshXvGkMhg",
                        QuizData = "[{\"question\":\"What is 'Resource Allocation'?\",\"options\":[{\"text\":\"Hiring more people\",\"isCorrect\":false},{\"text\":\"Assigning the right person to the right task\",\"isCorrect\":true},{\"text\":\"Buying more equipment\",\"isCorrect\":false},{\"text\":\"Increasing the budget\",\"isCorrect\":false}]}]"
                    }
                );
                context.SaveChanges();
            }

           
            var courses = context.Courses.ToList();
            foreach (var course in courses)
            {
                if (string.IsNullOrEmpty(course.QuizData) || course.QuizData.Length < 10)
                {
                    if (course.Title.Contains("Angular"))
                        course.QuizData = "[{\"question\":\"What is the primary benefit of Standalone Components?\",\"options\":[{\"text\":\"Reduced bundle size\",\"isCorrect\":false},{\"text\":\"Simplified dependencies without NgModules\",\"isCorrect\":true},{\"text\":\"Faster CSS loading\",\"isCorrect\":false},{\"text\":\"Automatic database syncing\",\"isCorrect\":false}]}]";
                    else if (course.Title.Contains(".NET"))
                        course.QuizData = "[{\"question\":\"Which .NET feature allows for building lightweight APIs?\",\"options\":[{\"text\":\"WebForms\",\"isCorrect\":false},{\"text\":\"Minimal APIs\",\"isCorrect\":true},{\"text\":\"Silverlight\",\"isCorrect\":false},{\"text\":\"WPF\",\"isCorrect\":false}]}]";
                    else if (course.Title.Contains("UI/UX"))
                        course.QuizData = "[{\"question\":\"What does UX stand for?\",\"options\":[{\"text\":\"User Xylophone\",\"isCorrect\":false},{\"text\":\"User Experience\",\"isCorrect\":true},{\"text\":\"Unit Exchange\",\"isCorrect\":false},{\"text\":\"Universal X-factor\",\"isCorrect\":false}]}]";
                    else if (course.Title.Contains("Leadership"))
                        course.QuizData = "[{\"question\":\"What are OKRs?\",\"options\":[{\"text\":\"Occasional Key Results\",\"isCorrect\":false},{\"text\":\"Objectives and Key Results\",\"isCorrect\":true},{\"text\":\"Online Key Resources\",\"isCorrect\":false},{\"text\":\"Optimal Knowledge Reports\",\"isCorrect\":false}]}]";
                    else if (course.Title.Contains("Conflict"))
                        course.QuizData = "[{\"question\":\"What is the 'Feedback Sandwich'?\",\"options\":[{\"text\":\"Bread, Meat, Bread\",\"isCorrect\":false},{\"text\":\"Positive, Constructive, Positive\",\"isCorrect\":true},{\"text\":\"Soft, Hard, Soft\",\"isCorrect\":false},{\"text\":\"Start, Stop, Continue\",\"isCorrect\":false}]}]";
                    else if (course.Title.Contains("Resource"))
                        course.QuizData = "[{\"question\":\"What is 'Resource Allocation'?\",\"options\":[{\"text\":\"Hiring more people\",\"isCorrect\":false},{\"text\":\"Assigning the right person to the right task\",\"isCorrect\":true},{\"text\":\"Buying more equipment\",\"isCorrect\":false},{\"text\":\"Increasing the budget\",\"isCorrect\":false}]}]";
                }

                
                if (course.Title.Contains("Angular")) course.VideoUrl = "https://www.youtube.com/embed/3qBXWUpoPHo";
                else if (course.Title.Contains(".NET")) course.VideoUrl = "https://www.youtube.com/embed/C5cnZ-gZy2I";
                else if (course.Title.Contains("UI/UX")) course.VideoUrl = "https://www.youtube.com/embed/c9Wg6Cb_YlU";
                else if (course.Title.Contains("Leadership")) course.VideoUrl = "https://www.youtube.com/embed/1G4S1R2X5l0";
                else if (course.Title.Contains("Conflict")) course.VideoUrl = "https://www.youtube.com/embed/WbjeC8aZXXE";
                else if (course.Title.Contains("Resource")) course.VideoUrl = "https://www.youtube.com/embed/s_xR4x8XW1Q";

                var existingModule = context.CourseModules.FirstOrDefault(m => m.CourseId == course.Id && m.OrderIndex == 1);
                if (existingModule != null && existingModule.Content.Contains("iframe"))
                {
                    existingModule.Content = $"<p>Welcome to this course. In this first module, we cover the fundamentals.</p><iframe width=\"100%\" height=\"400\" src=\"{course.VideoUrl}\" frameborder=\"0\" allowfullscreen></iframe>";
                }

                if (!context.CourseModules.Any(m => m.CourseId == course.Id))
                {
                    context.CourseModules.AddRange(
                        new CourseModule { CourseId = course.Id, Title = "Introduction to " + course.Title, Content = $"<p>Welcome to this course. In this first module, we cover the fundamentals.</p><iframe width=\"100%\" height=\"400\" src=\"{course.VideoUrl}\" frameborder=\"0\" allowfullscreen></iframe>", OrderIndex = 1 },
                        new CourseModule { CourseId = course.Id, Title = "Core Concepts", Content = "<p>This module dives deep into the core concepts and architecture.</p><p>Key takeaways:</p><ul><li>Architecture patterns</li><li>Best practices</li><li>Scalability</li></ul>", OrderIndex = 2 },
                        new CourseModule { CourseId = course.Id, Title = "Advanced Implementation", Content = "<p>Now we apply what we've learned to real-world scenarios.</p><div class='alert info'>Pay attention to the implementation details here.</div>", OrderIndex = 3 }
                    );
                }
            }
            context.SaveChanges();

            if (!context.Projects.Any())
            {
                context.Projects.Add(new Project
                {
                    Name = "E-Commerce Re-platforming",
                    Description = "Modernizing our legacy e-commerce system with a new Angular frontend and .NET Core backend. This project includes migrating 10 years of customer data and integrating new payment gateways.",
                    Budget = 50000,
                    Status = "Active",
                    DueDate = DateTime.Now.AddDays(30),
                    FilesUrl = ""
                });
                context.SaveChanges();
            }
        }

        private static void InitializeSchema(AppDbContext context)
        {
            context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'TargetRole') ALTER TABLE [Courses] ADD [TargetRole] nvarchar(max) NULL;");
            context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsMandatory') ALTER TABLE [Enrollments] ADD [IsMandatory] bit NOT NULL DEFAULT 0;");
            context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'AssignedById') ALTER TABLE [Enrollments] ADD [AssignedById] int NULL;");
            context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'DueDate') ALTER TABLE [Enrollments] ADD [DueDate] datetime2 NULL;");
            
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Messages')
                CREATE TABLE [Messages] (
                    [Id] int NOT NULL IDENTITY,
                    [SenderId] int NOT NULL,
                    [ReceiverId] int NOT NULL,
                    [Content] nvarchar(max) NULL,
                    [FileUrl] nvarchar(max) NULL,
                    [FileType] nvarchar(max) NULL,
                    [SentAt] datetime2 NOT NULL,
                    [IsRead] bit NOT NULL,
                    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Messages_Users_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Users] ([Id]),
                    CONSTRAINT [FK_Messages_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users] ([Id])
                );
                ELSE
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Messages]') AND name = 'FileUrl')
                        ALTER TABLE [Messages] ADD [FileUrl] nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Messages]') AND name = 'FileType')
                        ALTER TABLE [Messages] ADD [FileType] nvarchar(max) NULL;
                END
            ");
        }
    }
}

 
