using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    public class Food
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("food_id")]
        public int FoodId { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        [Column("calories")]
        public double Calories { get; set; }

        [Column("protein")]
        public double? Protein { get; set; }

        [Column("fat")]
        public double? Fat { get; set; }

        [Column("carbs")]
        public double? Carbs { get; set; }

        [Column("unit")]
        public string? Unit { get; set; }
    }
}
