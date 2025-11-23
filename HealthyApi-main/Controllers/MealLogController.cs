using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MealLogController : ControllerBase
    {
        private readonly DataContext _context;

        public MealLogController(DataContext context)
        {
            _context = context;
        }

        // ===================== 13. 添加食物记录 =====================
        [HttpPost("add")]
        public async Task<IActionResult> AddMealRecord([FromBody] MealRecordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1️⃣ 检查用户是否存在
            var user = await _context.Users.FindAsync(request.user_id);
            if (user == null)
                return NotFound("找不到对应的用户");

            // 2️⃣ 创建 MealLog
            var mealLog = new MealLog
            {
                UserId = request.user_id,
                Date = request.date.Date,
                MealType = request.meal_type switch
                {
                    "早餐" => "breakfast",
                    "午餐" => "lunch",
                    "晚餐" => "dinner",
                    "加餐" => "snack",
                    _ => "other"
                }
            };

            _context.MealLogs.Add(mealLog);
            await _context.SaveChangesAsync();

            // 3️⃣ 保存 MealFood
            foreach (var item in request.meals)
            {
                var food = await _context.Foods.FindAsync(item.food_id);
                if (food == null)
                    return BadRequest($"无效的 food_id: {item.food_id}");

                var mealFood = new MealFood
                {
                    MealId = mealLog.MealId,
                    FoodId = item.food_id,
                    Amount = item.grams
                };

                _context.MealFoods.Add(mealFood);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }


        // ===================== 14. 获取食物记录 =====================
        [HttpGet("records")]
        public async Task<IActionResult> GetMealRecords(
            [FromQuery] int user_id,
            [FromQuery] string mode = "all",
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null,
            [FromQuery] DateTime? date = null)
        {
            var query = _context.MealLogs
                .Include(m => m.MealFoods)
                .Where(m => m.UserId == user_id)
                .AsQueryable();

            // ===================== ① today 模式 =====================
            if (mode == "today" && date.HasValue)
            {
                var todayMealLogs = await query
                    .Where(m => m.Date == date.Value.Date)
                    .OrderBy(m => m.MealType)
                    .ToListAsync();

                var result = todayMealLogs.Select(m => new TodayMealDto
                {
                    MealType = m.MealType,
                    Items = m.MealFoods.Select(f =>
                    {
                        var food = _context.Foods.FirstOrDefault(x => x.FoodId == f.FoodId);
                        return new FoodItemDto
                        {
                            Id = f.MealFoodId,
                            Name = food?.Name ?? "",
                            Amount = f.Amount,
                            Calorie = food == null ? 0 : Math.Round(food.Calories * f.Amount / 100.0, 2)
                        };
                    }).ToList()
                }).ToList();

                var records = new
                {
                    breakfast = result.FirstOrDefault(x => x.MealType == "breakfast")?.Items ?? new List<FoodItemDto>(),
                    lunch = result.FirstOrDefault(x => x.MealType == "lunch")?.Items ?? new List<FoodItemDto>(),
                    dinner = result.FirstOrDefault(x => x.MealType == "dinner")?.Items ?? new List<FoodItemDto>(),
                    snack = result.FirstOrDefault(x => x.MealType == "snack")?.Items ?? new List<FoodItemDto>()
                };

                return Ok(new { records });
            }


            // ===================== ② week / month 模式处理 =====================
            if ((mode == "week" || mode == "month") && start.HasValue && end.HasValue)
            {
                query = query.Where(m => m.Date >= start.Value && m.Date <= end.Value);
            }

            // ===================== ③ all / week / month 共有查询 =====================
            var allLogs = await query
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            var grouped = allLogs
                .GroupBy(m => m.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    meals = g.Select(m => new
                    {
                        type = m.MealType,
                        items = m.MealFoods.Select(f =>
                        {
                            var food = _context.Foods.FirstOrDefault(x => x.FoodId == f.FoodId);
                            return new
                            {
                                id = f.MealFoodId,
                                name = food?.Name ?? "",
                                amount = f.Amount,
                                calorie = food == null ? 0 : Math.Round(food.Calories * f.Amount / 100.0, 2)
                            };
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            return Ok(grouped);
        }


        // ===================== 15. 删除食物记录 =====================
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMealRecord([FromBody] DeleteMealRequest req)
        {
            var mealFood = await _context.MealFoods
                .Include(f => f.MealLog)
                .FirstOrDefaultAsync(f =>
                    f.MealFoodId == req.food_id &&
                    f.MealLog.UserId == req.user_id);

            if (mealFood == null)
                return NotFound("记录不存在");

            var mealLog = mealFood.MealLog;

            _context.MealFoods.Remove(mealFood);
            await _context.SaveChangesAsync();

            bool hasRemainingFoods = await _context.MealFoods
                .AnyAsync(f => f.MealId == mealLog.MealId);

            if (!hasRemainingFoods)
            {
                _context.MealLogs.Remove(mealLog);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }


        // ======= DTO（数据结构）=======
        public class MealRecordRequest
        {
            public int user_id { get; set; }
            public DateTime date { get; set; }
            public string meal_type { get; set; } = "";
            public List<MealItemDto> meals { get; set; } = new();
        }

        public class MealItemDto
        {
            public int food_id { get; set; }
            public double grams { get; set; }
        }

        public class DeleteMealRequest
        {
            public int user_id { get; set; }
            public int food_id { get; set; }
        }

        public class FoodItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public double Amount { get; set; }
            public double Calorie { get; set; }
        }

        public class TodayMealDto
        {
            public string MealType { get; set; } = "";
            public List<FoodItemDto> Items { get; set; } = new();
        }
    }
}
