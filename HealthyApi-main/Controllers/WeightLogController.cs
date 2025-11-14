using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeightLogController : ControllerBase
    {
        private readonly DataContext _context;

        public WeightLogController(DataContext context)
        {
            _context = context;
        }

        // ===================== ⑥ 添加体重记录 =====================
        [HttpPost("add")]
        public async Task<IActionResult> AddWeightLog([FromBody] WeightLog log)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 确保日期有值
            log.Date = log.Date == default ? DateTime.Now : log.Date;

            // 获取用户信息（计算体脂率要用年龄、性别、身高）
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == log.UserId);
            if (user == null)
                return NotFound("找不到对应的用户");

            // 计算 BMI
            if (user.Height.HasValue && user.Height > 0)
            {
                double heightInMeter = user.Height.Value / 100.0; // cm → m
                double bmi = log.Weight / (heightInMeter * heightInMeter);

                // 计算体脂率
                double bodyFat;
                if (user.Gender == "M")
                    bodyFat = 1.2 * bmi + 0.23 * (user.Age ?? 25) - 5.4 - 10.8 * 1;
                else
                    bodyFat = 1.2 * bmi + 0.23 * (user.Age ?? 25) - 5.4;

                log.BodyFat = Math.Round(bodyFat, 2); // 保留两位小数
            }

            _context.WeightLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(); // HTTP 200，无返回体
        }

        // ===================== ⑦ 获取体重记录（支持 mode） =====================
        [HttpGet("records")]
        public IActionResult GetWeightLogs(
     [FromQuery] int UserId,
     [FromQuery] string mode = "all",
     [FromQuery] string? start = null,
     [FromQuery] string? end = null)
        {
            var query = _context.WeightLogs.Where(w => w.UserId == UserId);

            // ✅ 优先使用用户选择的日期范围（与饮水记录一致）
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var startDate = DateTime.Parse(start);
                var endDate = DateTime.Parse(end);
                query = query.Where(w => w.Date >= startDate && w.Date <= endDate);
            }
            else if (mode == "week")
            {
                query = query.Where(w => w.Date >= DateTime.Now.AddDays(-7));
            }
            else if (mode == "month")
            {
                query = query.Where(w => w.Date >= DateTime.Now.AddMonths(-1));
            }
            // mode == all 不过滤

            var logs = query
                .OrderByDescending(w => w.Date)
                .Select(w => new
                {
                    id = w.LogId,
                    date = w.Date.ToString("MM.dd"),  // ✅ 改成你 UI 想要的显示格式
                    time = w.TimeOfDay ?? "--",
                    weight = w.Weight,
                    bodyFat = w.BodyFat // ✅ 正确传输体脂率字段
                })
                .ToList();

            return Ok(logs);
        }


        // ===================== ⑧ 删除体重记录 =====================
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteWeightLog([FromBody] DeleteWeightRequest req)
        {
            var log = await _context.WeightLogs
                .FirstOrDefaultAsync(w => w.LogId == req.LogId && w.UserId == req.UserId);

            if (log == null)
                return NotFound("记录不存在");

            _context.WeightLogs.Remove(log);
            await _context.SaveChangesAsync();

            return Ok(); // HTTP200，无内容
        }
        /// <summary>
        /// --------------------
        /// </summary>
        // ===================== ⑨ 获取当前身体状态 =====================
        // GET: /api/WeightLog/body-info?UserId=123
        [HttpGet("body-info")]
        public async Task<IActionResult> GetBodyInfo([FromQuery] int UserId)
        {
            // 1) 查询用户基础信息
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == UserId);
            if (user == null)
                return NotFound("找不到对应的用户");

            double height = user.Height ?? 0;    // cm
            double weight = user.InitialWeight ?? 0;  // 默认用初始体重
            double? bodyFat = null;

            // 2) 查今日是否有体重记录
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            var todayLog = await _context.WeightLogs
                .Where(w => w.UserId == UserId && w.Date.ToString("yyyy-MM-dd") == today)
                .OrderByDescending(w => w.LogId)
                .FirstOrDefaultAsync();

            if (todayLog != null)
            {
                weight = todayLog.Weight;
                bodyFat = todayLog.BodyFat;
            }

            // 3) 计算 BMI
            double heightM = height / 100.0;
            double bmi = (heightM > 0) ? weight / (heightM * heightM) : 0;

            // 4) 计算 BMR（基础代谢）
            double BMR;
            if (user.Gender == "M")
                BMR = 10 * weight + 6.25 * height - 5 * (user.Age ?? 25) + 5;
            else
                BMR = 10 * weight + 6.25 * height - 5 * (user.Age ?? 25) - 161;

            // 5) 返回结果
            return Ok(new
            {
                weight = Math.Round(weight, 1),
                height = height,
                bmr = Math.Round(BMR),
                body_fat = bodyFat,
                bmi = Math.Round(bmi, 1)
            });
        }
        // ===================== ⑩ 获取每日热量统计 =====================
        // GET: /api/WeightLog/daily-calorie?UserId=3&date=2025-10-25
        [HttpGet("daily-calorie")]
        public async Task<IActionResult> GetDailyCalorie(
            [FromQuery] int UserId,
            [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out DateTime targetDate))
                return BadRequest("Invalid date format.");

            string dateStr = targetDate.ToString("yyyy-MM-dd");

            // ========== 1) 获取用户基本信息 ==========
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == UserId);
            if (user == null)
                return NotFound("用户不存在");

            double height = user.Height ?? 0;
            double weight = user.InitialWeight ?? 0;
            int age = user.Age ?? 25;
            string gender = user.Gender;

            // ========== 2) 今日体重（优先今日 WeightLog） ==========
            var todayLog = await _context.WeightLogs
                .Where(w => w.UserId == UserId &&
                            w.Date.ToString("yyyy-MM-dd") == dateStr)
                .OrderByDescending(w => w.LogId)
                .FirstOrDefaultAsync();

            if (todayLog != null)
                weight = todayLog.Weight;

            // ========== 3) BMR 计算 ==========
            double BMR;
            if (gender == "M")
                BMR = 10 * weight + 6.25 * height - 5 * age + 5;
            else
                BMR = 10 * weight + 6.25 * height - 5 * age - 161;

            // ========== 4) 今日运动消耗 ==========
            double exerciseKcal = await _context.ExerciseLogs
                .Where(e => e.UserId == UserId &&
                            e.Date.ToString("yyyy-MM-dd") == dateStr)
                .SumAsync(e => (double?)e.TotalCalories ?? 0);

            // ========== 5) 今日可吃总量（TDEE）==========
            double TDEE = BMR * 1.2 + exerciseKcal;

            // ========== 6) 查询今天餐次 ==========
            var mealLogs = await _context.MealLogs
                .Where(m => m.UserId == UserId &&
                            m.Date.ToString("yyyy-MM-dd") == dateStr)
                .ToListAsync();

            if (!mealLogs.Any())
            {
                return Ok(new
                {
                    remain_calorie = Math.Round(TDEE),
                    breakfast_kcal = 0,
                    lunch_kcal = 0,
                    dinner_kcal = 0,
                    snack_kcal = 0
                });
            }

            var mealIds = mealLogs.Select(m => m.MealId).ToList();

            // ========== 7) MealFoods + Foods 联查 ==========
            var foodRecords = await _context.MealFoods
                .Where(mf => mealIds.Contains(mf.MealId))
                .Join(_context.Foods,
                    mf => mf.FoodId,
                    f => f.FoodId,
                    (mf, f) => new
                    {
                        mf.MealId,
                        mf.Amount,
                        f.Calories
                    })
                .ToListAsync();

            // 工具函数：算某一餐 kcal
            double GetKcal(string type)
            {
                var ids = mealLogs
                    .Where(m => m.MealType == type)
                    .Select(m => m.MealId)
                    .ToList();

                return foodRecords
                    .Where(r => ids.Contains(r.MealId))
                    .Sum(r => r.Calories * r.Amount / 100.0);
            }

            double breakfast = GetKcal("breakfast");
            double lunch = GetKcal("lunch");
            double dinner = GetKcal("dinner");
            double snack = GetKcal("snack");

            double eatKcal = breakfast + lunch + dinner + snack;

            // ========== 8) remain_calorie ==========
            double remain = TDEE - eatKcal;
            if (remain < 0) remain = 0;

            return Ok(new
            {
                remain_calorie = Math.Round(remain),
                breakfast_kcal = Math.Round(breakfast),
                lunch_kcal = Math.Round(lunch),
                dinner_kcal = Math.Round(dinner),
                snack_kcal = Math.Round(snack)
            });
        }


        // DTO: 删除请求格式
        public class DeleteWeightRequest
        {
            public int UserId { get; set; }
            public int LogId { get; set; }
        }
    }
}
