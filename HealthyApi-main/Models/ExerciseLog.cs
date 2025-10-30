using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    // 为 user_id + date 创建复合索引，加快查询速度
    [Index(nameof(UserId), nameof(Date))]
    public class ExerciseLog
    {
        // 主键：自增
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("exercise_id")]
        public int ExerciseId { get; set; }

        // 外键：用户 ID
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        // 日期（非空）
        [Required]
        [Column("date")]
        public DateTime Date { get; set; }

        // 开始时间（可空）
        [Column("start_time")]
        public string? StartTime { get; set; }

        // 运动名称（非空）
        [Required]
        [Column("exercise_name")]
        public string ExerciseName { get; set; }

        // 单位热量（kcal/分钟或/次，可空）
        [Column("calories_per_unit")]
        public double? CaloriesPerUnit { get; set; }

        // 时长（分钟）或次数
        [Column("duration")]
        public double? Duration { get; set; }

        // 实际消耗热量（kcal）
        [Column("total_calories")]
        public double? TotalCalories { get; set; }

        // 外键导航属性（可选）
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
