using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    public class MealFood
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("meal_food_id")]
        public int MealFoodId { get; set; }

        [Required]
        [Column("meal_id")]
        public int MealId { get; set; }

        [Required]
        [Column("food_id")]
        public int FoodId { get; set; }   // 不可再为空

        [Required]
        [Column("amount")]
        public double Amount { get; set; }   // 克数

        // 外键
        [ForeignKey("MealId")]
        public MealLog? MealLog { get; set; }

        [ForeignKey("FoodId")]
        public Food? Food { get; set; }
    }
}
