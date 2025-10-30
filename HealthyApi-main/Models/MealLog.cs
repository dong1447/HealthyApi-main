using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    // 用户每天的餐次日志（早餐、午餐、晚餐、加餐）
    [Index(nameof(UserId), nameof(Date))]
    public class MealLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("meal_id")]
        public int MealId { get; set; }

        // 外键：用户ID
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        // 日期（例如：2025-10-25）
        [Required]
        [Column("date")]
        public DateTime Date { get; set; }

        // 餐次类型（breakfast / lunch / dinner / snack）
        [Required]
        [Column("meal_type")]
        public string MealType { get; set; }

        // 备注，可选
        [Column("note")]
        public string? Note { get; set; }

        // 一顿餐对应多个食物
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();

        // 外键导航属性
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
