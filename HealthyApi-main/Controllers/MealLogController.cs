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

            // 2️⃣ 创建新的餐次记录（MealLog）
            var mealLog = new MealLog
            {
                UserId = request.user_id,
                Date = request.date,
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
            await _context.SaveChangesAsync(); // 保存后生成 meal_id

            // 3️⃣ 遍历 meals 数组，为每个食物创建 MealFood 记录
            foreach (var food in request.meals)
            {
                var mealFood = new MealFood
                {
                    MealId = mealLog.MealId,
                    FoodName = food.name,
                    Amount = food.grams,
                    Calorie = food.calorie
                };
                _context.MealFoods.Add(mealFood);
            }

            await _context.SaveChangesAsync();

            // ✅ 前端无需返回内容，只返回 HTTP200
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

            // 1) today 模式
            if (mode == "today" && date.HasValue)
            {
                var todayMeals = await query
                    .Where(m => m.Date == date.Value.Date)
                    .OrderBy(m => m.MealType)
                    .Select(m => new TodayMealDto
                    {
                        MealType = m.MealType,
                        Items = m.MealFoods.Select(f => new FoodItemDto
                        {
                            Id = f.MealFoodId,
                            Name = f.FoodName,
                            Amount = f.Amount,
                            Calorie = f.Calorie
                        }).ToList()
                    })
                    .ToListAsync();

                var records = new
                {
                    breakfast = todayMeals.FirstOrDefault(x => x.MealType == "breakfast")?.Items ?? new List<FoodItemDto>(),
                    lunch = todayMeals.FirstOrDefault(x => x.MealType == "lunch")?.Items ?? new List<FoodItemDto>(),
                    dinner = todayMeals.FirstOrDefault(x => x.MealType == "dinner")?.Items ?? new List<FoodItemDto>(),
                    snack = todayMeals.FirstOrDefault(x => x.MealType == "snack")?.Items ?? new List<FoodItemDto>()
                };

                return Ok(new { records });
            }

            // 2) week / month 模式
            if ((mode == "week" || mode == "month") && start.HasValue && end.HasValue)
                query = query.Where(m => m.Date >= start.Value && m.Date <= end.Value);

            // 3) all 模式
            var grouped = await query
                .OrderByDescending(m => m.Date)
                .GroupBy(m => m.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    meals = g.Select(m => new
                    {
                        type = m.MealType,
                        items = m.MealFoods.Select(f => new
                        {
                            id = f.MealFoodId,
                            name = f.FoodName,
                            amount = f.Amount,
                            calorie = f.Calorie
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(grouped);
        }

        // ===================== 15. 删除食物记录 =====================
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMealRecord([FromBody] DeleteMealRequest req)
        {
            var mealFood = await _context.MealFoods
                .Include(f => f.MealLog)
                .FirstOrDefaultAsync(f => f.MealFoodId == req.food_id && f.MealLog.UserId == req.user_id);

            if (mealFood == null)
                return NotFound("记录不存在");

            _context.MealFoods.Remove(mealFood);
            await _context.SaveChangesAsync();

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
            public string name { get; set; } = "";
            public double grams { get; set; }
            public double calorie { get; set; }
        }

        public class DeleteMealRequest
        {
            public int user_id { get; set; }
            public int food_id { get; set; }
        }

        private class FoodItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public double Amount { get; set; }
            public double Calorie { get; set; }
        }

        private class TodayMealDto
        {
            public string MealType { get; set; } = "";
            public List<FoodItemDto> Items { get; set; } = new();
        }
    }
}
