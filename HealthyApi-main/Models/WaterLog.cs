using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthyApi.Models
{
    [Index(nameof(UserId), nameof(Date))] // 联合索引
    public class WaterLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WaterId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }   // 日期

        public TimeSpan? Time { get; set; }  // 具体时间（可空）

        [Required]
        [MaxLength(50)]
        public string WaterType { get; set; } // 水、茶、咖啡、奶昔、红酒等

        [Required]
        public double Amount { get; set; } // ml

        // 导航属性
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
