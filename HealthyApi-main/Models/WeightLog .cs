using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace HealthyApi.Models
{
    [Index(nameof(UserId), nameof(Date))] // 
    public class WeightLog
    {
        // 主键：自增整数
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }  // 

        // 外键：关联 User 表的 UserId
        [Required]
        public int UserId { get; set; }

        // 日期：非空
        [Required]
        public DateTime Date { get; set; }

        // 一天中的时间（可选）
        public string? TimeOfDay { get; set; } // ✅ 可空引用类型，早餐前/午餐前/睡前...

        // 体重（非空，单位：kg）
        [Required]
        public double Weight { get; set; }

        // 体脂率（可选，单位：%）
        public double? BodyFat { get; set; }

        // 备注（可选）
        public string? Note { get; set; }

        // 导航属性：关联 User 实体
        [JsonIgnore]                        // ✅ 告诉序列化器忽略该字段
        [ForeignKey("UserId")]
        public User? User { get; set; }     // ✅ 显式允许为空
    }
}
