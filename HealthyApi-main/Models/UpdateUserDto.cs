using System;

namespace HealthyApi.Models
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }   // 必须，用来找是谁在更新
        public string Username { get; set; }
        public string Gender { get; set; }  // "M" / "F"
        public int? Age { get; set; }
        public double? Height { get; set; }
        public double? Initial_Weight { get; set; }
        public double? Target_Weight { get; set; }
    }
}
