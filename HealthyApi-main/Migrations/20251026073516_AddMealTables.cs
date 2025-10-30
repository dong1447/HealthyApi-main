using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMealTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "foods",
                columns: table => new
                {
                    food_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    category = table.Column<string>(type: "TEXT", nullable: true),
                    calories = table.Column<double>(type: "REAL", nullable: false),
                    protein = table.Column<double>(type: "REAL", nullable: true),
                    fat = table.Column<double>(type: "REAL", nullable: true),
                    carbs = table.Column<double>(type: "REAL", nullable: true),
                    unit = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_foods", x => x.food_id);
                });

            migrationBuilder.CreateTable(
                name: "meal_logs",
                columns: table => new
                {
                    meal_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    meal_type = table.Column<string>(type: "TEXT", nullable: false),
                    note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_logs", x => x.meal_id);
                    table.ForeignKey(
                        name: "FK_meal_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meal_foods",
                columns: table => new
                {
                    meal_food_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    meal_id = table.Column<int>(type: "INTEGER", nullable: false),
                    food_id = table.Column<int>(type: "INTEGER", nullable: true),
                    food_name = table.Column<string>(type: "TEXT", nullable: false),
                    amount = table.Column<double>(type: "REAL", nullable: false),
                    calorie = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_foods", x => x.meal_food_id);
                    table.ForeignKey(
                        name: "FK_meal_foods_foods_food_id",
                        column: x => x.food_id,
                        principalTable: "foods",
                        principalColumn: "food_id");
                    table.ForeignKey(
                        name: "FK_meal_foods_meal_logs_meal_id",
                        column: x => x.meal_id,
                        principalTable: "meal_logs",
                        principalColumn: "meal_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meal_foods_food_id",
                table: "meal_foods",
                column: "food_id");

            migrationBuilder.CreateIndex(
                name: "IX_meal_foods_meal_id",
                table: "meal_foods",
                column: "meal_id");

            migrationBuilder.CreateIndex(
                name: "IX_meal_logs_user_id_date",
                table: "meal_logs",
                columns: new[] { "user_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meal_foods");

            migrationBuilder.DropTable(
                name: "foods");

            migrationBuilder.DropTable(
                name: "meal_logs");
        }
    }
}
