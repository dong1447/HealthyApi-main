using HealthyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HealthyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodRecognitionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly HttpClient _httpClient;

        public FoodRecognitionController(DataContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // ===================== (1) 上传图片识别 =====================
        [HttpPost("recognize")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RecognizeFood([FromForm] FoodUploadDto req)
        {
            var image = req.Image;
            var user_id = req.UserId;
            var meal_type = req.MealType; 

            if (image == null || image.Length == 0)
                return BadRequest("未上传图片");

            try
            {
                // 上传并识别
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"{Guid.NewGuid()}.jpg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                var foods = await RecognizeFromBaiduAsync(filePath);

                // 这里只识别，不保存到数据库（因为还没选餐别）
                return Ok(new
                {
                    message = "识别成功",
                    items = foods.Select(f => new { cn = f.NameCn, kr = f.NameKr, kcal = f.Calories })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器错误", detail = ex.Message });
            }
        }

        // ===================== (2) 调百度菜品识别 =====================
        private async Task<List<FoodItem>> RecognizeFromBaiduAsync(string imagePath)
        {
            string accessToken = await GetBaiduAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("access_token 获取失败，请检查 API Key / Secret Key");

            string json = await CallDishApiAsync(accessToken, imagePath);
            Console.WriteLine("【百度识别返回】" + json);

            using var doc = JsonDocument.Parse(json);
            var list = new List<FoodItem>();

            // 检查是否报错
            if (doc.RootElement.TryGetProperty("error_code", out var err))
            {
                var msg = doc.RootElement.TryGetProperty("error_msg", out var em)
                    ? em.GetString() : "未知错误";
                Console.WriteLine($"百度识别接口错误：{err} - {msg}");
                return list;
            }

            // 解析识别结果
            if (doc.RootElement.TryGetProperty("result", out var results))
            {
                foreach (var item in results.EnumerateArray())
                {
                    string cn = item.GetProperty("name").GetString() ?? "";
                    double cal = 0;
                    if (item.TryGetProperty("calorie", out var c))
                        double.TryParse(c.GetString(), out cal);

                    string kr = await TranslateToKorean(cn);
                    list.Add(new FoodItem { NameCn = cn, NameKr = kr, Calories = cal });
                }
            }

            return list;
        }

        // ===================== (3) 获取 Access Token =====================
        private async Task<string> GetBaiduAccessTokenAsync()
        {
            string ak = "sjnmbxCQVqYelV5PiVIJ9IV7";   // 你的API Key
            string sk = "Fv7cURgNphGsE75JqDr8rUZt9waLFje8"; // 你的Secret Key

            var url = $"https://aip.baidubce.com/oauth/2.0/token" +
                      $"?grant_type=client_credentials&client_id={ak}&client_secret={sk}";

            var resp = await _httpClient.GetAsync(url);
            string txt = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("【百度 Token 返回】" + txt);

            try
            {
                using var doc = JsonDocument.Parse(txt);
                if (doc.RootElement.TryGetProperty("access_token", out var token))
                    return token.GetString() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token 解析失败：" + ex.Message);
            }

            return "";
        }

        // ===================== (4) 调菜品识别接口 =====================
        private async Task<string> CallDishApiAsync(string accessToken, string imagePath)
        {
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            string base64 = Convert.ToBase64String(bytes);
            string encoded = WebUtility.UrlEncode(base64);

            string body = $"image={encoded}&top_num=5";
            string url = $"https://aip.baidubce.com/rest/2.0/image-classify/v2/dish?access_token={accessToken}";

            var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var resp = await _httpClient.PostAsync(url, content);
            return await resp.Content.ReadAsStringAsync();
        }

        // ===================== (5) 中文 -> 韩文翻译 =====================
        private async Task<string> TranslateToKorean(string text)
        {
            string ak = "sjnmbxCQVqYeIV5PIVIJ9IV7";
            string sk = "MdTvV021X4GbaENGr2mMsYHItxOfJ93A";

            string token = await GetBaiduAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return text;

            string transUrl = $"https://aip.baidubce.com/rpc/2.0/mt/texttrans/v1?access_token={token}";

            var body = new
            {
                from = "zh",
                to = "kor",
                q = text
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync(transUrl, content);
            string json = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("【baidu】" + json);

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("result", out var result) &&
                    result.TryGetProperty("trans_result", out var arr))
                    return arr[0].GetProperty("dst").GetString() ?? text;
            }
            catch
            {
                return text;
            }

            return text;
        }
        [HttpPost("save")]
        public async Task<IActionResult> SaveMeal([FromBody] SaveMealRequest req)
        {
            var mealLog = new MealLog
            {
                UserId = req.UserId,
                Date = DateTime.Now.Date,
                MealType = req.MealType,
                Note = "AI"
            };
            _context.MealLogs.Add(mealLog);
            await _context.SaveChangesAsync();

            foreach (var item in req.Foods)
            {
                _context.MealFoods.Add(new MealFood
                {
                    MealId = mealLog.MealId,
                    FoodName = item.FoodName,
                    Calorie = item.Calorie,
                    Amount = item.Amount
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "保存成功", meal_id = mealLog.MealId });
        }

        public class SaveMealRequest
        {
            public int UserId { get; set; }
            public string MealType { get; set; } = "";
            public List<FoodDto> Foods { get; set; } = new();
        }

        public class FoodDto
        {
            public string FoodName { get; set; } = "";
            public double Calorie { get; set; }
            public int Amount { get; set; } = 100;
        }

        // ===================== (6) 内部DTO =====================
        private class FoodItem
        {
            public string NameCn { get; set; } = "";
            public string NameKr { get; set; } = "";
            public double Calories { get; set; }
        }
    }
}
