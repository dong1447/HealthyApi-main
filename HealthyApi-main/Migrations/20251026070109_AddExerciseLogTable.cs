using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise_logs",
                columns: table => new
                {
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    start_time = table.Column<string>(type: "TEXT", nullable: true),
                    exercise_name = table.Column<string>(type: "TEXT", nullable: false),
                    calories_per_unit = table.Column<double>(type: "REAL", nullable: true),
                    duration = table.Column<double>(type: "REAL", nullable: true),
                    total_calories = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_logs", x => x.exercise_id);
                    table.ForeignKey(
                        name: "FK_exercise_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_logs_user_id_date",
                table: "exercise_logs",
                columns: new[] { "user_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_logs");
        }
    }
}
