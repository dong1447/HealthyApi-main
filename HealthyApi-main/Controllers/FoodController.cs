using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly DataContext _context;

        public FoodController(DataContext context)
        {
            _context = context;
        }

        // ========== (1) 食物分类列表 ==========
        // GET: /api/Food/list-by-category?category=水果
        [HttpGet("list-by-category")]
        public async Task<IActionResult> GetFoodByCategory([FromQuery] string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Ok(new { foods = new List<object>() });

            var foods = await _context.Foods
                .Where(f => f.Category == category)
                .Select(f => new
                {
                    food_id = f.FoodId,
                    name = f.Name,
                    calories = f.Calories,
                    carbs = f.Carbs,
                    protein = f.Protein,
                    fat = f.Fat
                })
                .ToListAsync();

            return Ok(new { foods });
        }

        // ========== (2) 模糊搜索 ==========
        // GET: /api/Food/search?keyword=鸡
        [HttpGet("search")]
        public async Task<IActionResult> SearchFood([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new { foods = new List<object>() });

            keyword = keyword.Trim();

            var foods = await _context.Foods
                .Where(f => f.Name.Contains(keyword))
                .Select(f => new
                {
                    food_id = f.FoodId,
                    name = f.Name,
                    calories = f.Calories,
                    carbs = f.Carbs,
                    protein = f.Protein,
                    fat = f.Fat
                })
                .ToListAsync();

            return Ok(new { foods });
        }
    }
}
