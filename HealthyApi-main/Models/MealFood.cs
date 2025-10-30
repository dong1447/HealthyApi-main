using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    // 餐次中每种食物的具体记录
    public class MealFood
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("meal_food_id")]
        public int MealFoodId { get; set; }

        // 关联哪一顿餐
        [Required]
        [Column("meal_id")]
        public int MealId { get; set; }

        // 可选外键：未来可以关联 Food 表（现在可空）
        [Column("food_id")]
        public int? FoodId { get; set; }

        // 当前食物的名称（前端写死或用户输入）
        [Required]
        [Column("food_name")]
        public string FoodName { get; set; }

        // 实际吃了多少（单位：g/ml/份）
        [Column("amount")]
        public double Amount { get; set; }

        // 实际热量（kcal）
        [Column("calorie")]
        public double Calorie { get; set; }

        // 外键关系
        [ForeignKey("MealId")]
        public MealLog? MealLog { get; set; }

        [ForeignKey("FoodId")]
        public Food? Food { get; set; }
    }
}
