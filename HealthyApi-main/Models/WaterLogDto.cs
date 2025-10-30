using System;

namespace HealthyApi.Models
{
    public class WaterLogDto
    {
        public int User_Id { get; set; }       // 用户ID
        public string Date { get; set; }       // "yyyy-MM-dd"
        public string Time { get; set; }       // "HH:mm:ss"
        public string Water_Type { get; set; } // 饮品名称
        public double Amount { get; set; }     // ml
    }
}
