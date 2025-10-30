using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthyApi.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        public int? Age { get; set; }

        [Column(TypeName = "TEXT")]
        [RegularExpression(@"^(M|F|O)$", ErrorMessage = "Gender must be 'M', 'F', or 'O'")]
        public string Gender { get; set; }

        [Column("height")]
        public double? Height { get; set; }

        [Column("initial_weight")]
        public double? InitialWeight { get; set; }

        [Column("target_weight")]
        public double? TargetWeight { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 导航属性（1对多）
        public ICollection<WeightLog> WeightLogs { get; set; } = new HashSet<WeightLog>();
    }
}
