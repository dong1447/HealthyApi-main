using Microsoft.EntityFrameworkCore;
using HealthyApi.Models;

namespace HealthyApi
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }  // 用户表
        public DbSet<WeightLog> WeightLogs { get; set; }//添加体重日志

        public DbSet<WaterLog> WaterLogs { get; set; }
        public DbSet<ExerciseLog> ExerciseLogs { get; set; }//添加运动日志

        public DbSet<MealLog> MealLogs { get; set; }//哪天、哪一餐（早餐/午餐/晚餐/加餐）
        public DbSet<MealFood> MealFoods { get; set; }//这餐中具体吃了哪些食物、吃了多少
        public DbSet<Food> Foods { get; set; }//食物本身的营养信息（100g 的热量、蛋白质等）
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ 统一将表名和列名转换为小写 + 下划线（snake_case）
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // 表名转 snake_case
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entity.SetTableName(ToSnakeCase(tableName));
                }

                // 列名转 snake_case
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }

        // 🧩 工具函数：把 PascalCase 转成 snake_case
        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0) sb.Append('_');
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }


    }
}