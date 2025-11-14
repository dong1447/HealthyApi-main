using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthyApi.Models
{
    public class FoodUploadDto
    {
        [FromForm(Name = "image")]
        public IFormFile Image { get; set; }

        [FromForm(Name = "user_id")]
        public int UserId { get; set; }

        [FromForm(Name = "meal_type")]
        public string? MealType { get; set; } = null;
    }
}
