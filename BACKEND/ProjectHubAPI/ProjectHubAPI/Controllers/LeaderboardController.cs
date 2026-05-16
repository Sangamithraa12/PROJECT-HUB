using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;

namespace ProjectHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaderboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLeaderboard()
        {
            var leaderboard = await _context.Enrollments
                .Include(e => e.User)
                .GroupBy(e => new { e.UserId, e.User.Name })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.Name,
                    CoursesCompleted = g.Count(e => e.IsCompleted),
                    TotalProgress = g.Sum(e => e.ProgressPercentage),
                    AvgQuizScore = g.Average(e => (double?)e.QuizScore) ?? 0
                })
                .OrderByDescending(x => x.CoursesCompleted)
                .ThenByDescending(x => x.TotalProgress)
                .Take(10)
                .ToListAsync();

            return Ok(leaderboard);
        }
    }
}
 
