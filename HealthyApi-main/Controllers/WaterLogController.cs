using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthyApi.Models;
namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaterLogController : ControllerBase
    {
        private readonly DataContext _context;

        public WaterLogController(DataContext context)
        {
            _context = context;
        }

        // ===================== ③ 添加饮水记录 =====================
        // POST: api/WaterLog/add
        [HttpPost("add")]
        public async Task<IActionResult> AddWaterLog([FromBody] WaterLogDto dto)
        {
            var log = new WaterLog
            {
                UserId = dto.User_Id,
                Date = DateTime.Parse(dto.Date),
                Time = TimeSpan.Parse(dto.Time),
                WaterType = dto.Water_Type,
                Amount = dto.Amount
            };

            _context.WaterLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "记录成功" });
        }

        // ===================== ④ 获取饮水记录 =====================
        // GET: api/WaterLog/records
        [HttpGet("records")]
        public async Task<IActionResult> GetWaterLogs(
            [FromQuery] int userId,
            [FromQuery] string mode,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.WaterLogs
                .Where(w => w.UserId == userId)
                .AsQueryable();

            // ① today 模式：单天记录
            if (mode == "today" && date.HasValue)
            {
                var records = await query
                    .Where(w => w.Date == date.Value.Date)
                    .ToListAsync(); // ✅ 先拉回内存

                // ✅ 在内存排序
                records = records
                    .OrderBy(w => w.Time.HasValue ? w.Time.Value.Ticks : 0)
                    .ToList();

                return Ok(new
                {
                    date = date.Value.ToString("yyyy-MM-dd"),
                    records = records.Select(w => new
                    {
                        id = w.WaterId,
                        time = w.Time,
                        drink = w.WaterType,
                        amount = w.Amount
                    }).ToList()
                });
            }


            // ② week / month 模式：日期区间
            if ((mode == "week" || mode == "month") && startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(w => w.Date >= startDate && w.Date <= endDate);
            }

            // ③ all 模式：返回全部记录
            // （不修改）

            // 分组返回结果
            // ✅ 关键：先从数据库取出，再在内存排序（避免 SQLite 无法排序 TimeSpan）
            var grouped = query
    .AsEnumerable() // ✅ 转为内存 LINQ
    .OrderByDescending(w => w.Date)
    .ThenBy(w => w.Time.HasValue ? w.Time.Value.Ticks : 0)
    .GroupBy(w => w.Date)
    .Select(g => new
    {
        date = g.Key.ToString("yyyy-MM-dd"),
        records = g.Select(w => new
        {
            id = w.WaterId,
            time = w.Time,
            drink = w.WaterType,
            amount = w.Amount
        }).ToList()
    })
    .ToList();

            return Ok(new { data = grouped });

        }


        // ===================== ⑤ 删除饮水记录 =====================
        // POST: api/WaterLog/delete
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteWaterLog([FromBody] DeleteWaterRequest req)
        {
            var record = await _context.WaterLogs
                .FirstOrDefaultAsync(w => w.WaterId == req.RecordId && w.UserId == req.UserId);

            if (record == null)
                return NotFound("记录不存在");

            _context.WaterLogs.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(); // HTTP200，无返回体
        }
    }

    // DTO 删除请求结构
    public class DeleteWaterRequest
    {
        public int UserId { get; set; }
        public int RecordId { get; set; }
    }
}
