using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseLogController : ControllerBase
    {
        private readonly DataContext _context;

        public ExerciseLogController(DataContext context)
        {
            _context = context;
        }

        // ===================== 10. 添加运动记录 =====================
        // POST: api/ExerciseLog/add
        [HttpPost("add")]
        public async Task<IActionResult> AddExerciseLog([FromBody] ExerciseRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Activities == null || !request.Activities.Any())
                return BadRequest("活动列表不能为空");

            foreach (var activity in request.Activities)
            {
                var log = new ExerciseLog
                {
                    UserId = request.UserId,
                    Date = request.Date,
                    StartTime = null,
                    ExerciseName = activity.Name,
                    Duration = activity.Time,
                    TotalCalories = activity.Calorie,
                    CaloriesPerUnit = null,
                };

                _context.ExerciseLogs.Add(log);
            }

            await _context.SaveChangesAsync();

            return Ok(); // HTTP200，无需返回体
        }

        // ===================== 11. 获取运动记录 =====================
        // GET: api/ExerciseLog/records
        [HttpGet("records")]
        public async Task<IActionResult> GetExerciseLogs(
            [FromQuery] int user_id,
            [FromQuery] string mode = "all",
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null,
            [FromQuery] DateTime? date = null)
        {
            var query = _context.ExerciseLogs
                .Where(e => e.UserId == user_id)
                .AsQueryable();

            // today 模式
            if (mode == "today" && date.HasValue)
            {
                var activities = await query
                    .Where(e => e.Date == date.Value.Date)
                    .OrderBy(e => e.ExerciseId)
                    .Select(e => new
                    {
                        record_id = e.ExerciseId,
                        name = e.ExerciseName,
                        time = e.Duration,
                        calorie = e.TotalCalories,
                        tag = "운동"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    date = date.Value.ToString("yyyy-MM-dd"),
                    activities
                });
            }

            // week / month 模式
            if ((mode == "week" || mode == "month") && start.HasValue && end.HasValue)
            {
                query = query.Where(e => e.Date >= start.Value && e.Date <= end.Value);
            }

            // all 模式不过滤
            var grouped = await query
                .OrderByDescending(e => e.Date)
                .GroupBy(e => e.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    activities = g.Select(e => new
                    {
                        record_id = e.ExerciseId,
                        name = e.ExerciseName,
                        time = e.Duration,
                        calorie = e.TotalCalories,
                        tag = "운동"
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { data = grouped });
        }

        // ===================== 12. 删除运动记录 =====================
        // POST: api/ExerciseLog/delete
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteExerciseLog([FromBody] DeleteExerciseRequest req)
        {
            var record = await _context.ExerciseLogs
                .FirstOrDefaultAsync(e => e.ExerciseId == req.record_id && e.UserId == req.user_id);

            if (record == null)
                return NotFound("记录不存在");

            _context.ExerciseLogs.Remove(record);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    // ===================== DTO 模型 =====================

    public class ExerciseRequest
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public List<ActivityItem> Activities { get; set; }
    }

    public class ActivityItem
    {
        public string Name { get; set; }
        public double Time { get; set; }
        public double Calorie { get; set; }
        public string Tag { get; set; }
    }

    public class DeleteExerciseRequest
    {
        public int user_id { get; set; }
        public int record_id { get; set; }
    }
}
