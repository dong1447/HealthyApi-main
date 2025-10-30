using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthyApi.Migrations
{
    /// <inheritdoc />
    public partial class InitAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", nullable: false),
                    age = table.Column<int>(type: "INTEGER", nullable: true),
                    gender = table.Column<string>(type: "TEXT", nullable: false),
                    height = table.Column<double>(type: "REAL", nullable: true),
                    initial_weight = table.Column<double>(type: "REAL", nullable: true),
                    target_weight = table.Column<double>(type: "REAL", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "water_logs",
                columns: table => new
                {
                    water_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    water_type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    amount = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_water_logs", x => x.water_id);
                    table.ForeignKey(
                        name: "FK_water_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weight_logs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time_of_day = table.Column<string>(type: "TEXT", nullable: true),
                    weight = table.Column<double>(type: "REAL", nullable: false),
                    body_fat = table.Column<double>(type: "REAL", nullable: true),
                    note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weight_logs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_weight_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_water_logs_user_id_date",
                table: "water_logs",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_weight_logs_user_id_date",
                table: "weight_logs",
                columns: new[] { "user_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "water_logs");

            migrationBuilder.DropTable(
                name: "weight_logs");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
