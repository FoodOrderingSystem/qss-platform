using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAndRoomMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ProcessCategoryItems table
            migrationBuilder.CreateTable(
                name: "ProcessCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessCategories", x => x.Id);
                });

            // Seed default categories matching the legacy ProcessCategory enum order
            // Enum: Hygiene=0, Maintenance=1, Administrative=2, Clinical=3, Safety=4, Training=5, Other=6
            // IDs will be 1–7 matching Category + 1
            migrationBuilder.InsertData(
                table: "ProcessCategories",
                columns: new[] { "Id", "Name", "Description", "IsActive", "SortOrder", "CreatedAt", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Hygiene", "Hygiene and sterilization procedures", true, 0, DateTime.UtcNow, false },
                    { 2, "Maintenance", "Equipment and facility maintenance", true, 1, DateTime.UtcNow, false },
                    { 3, "Administrative", "Administrative and documentation processes", true, 2, DateTime.UtcNow, false },
                    { 4, "Clinical", "Clinical treatment procedures", true, 3, DateTime.UtcNow, false },
                    { 5, "Safety", "Safety and compliance procedures", true, 4, DateTime.UtcNow, false },
                    { 6, "Training", "Staff training and development", true, 5, DateTime.UtcNow, false },
                    { 7, "Other", "Other processes", true, 6, DateTime.UtcNow, false }
                });

            // Add ProcessCategoryId column to QssProcesses
            migrationBuilder.AddColumn<int>(
                name: "ProcessCategoryId",
                table: "Processes",
                type: "INTEGER",
                nullable: true);

            // Map existing processes: ProcessCategoryId = legacy Category enum value + 1
            migrationBuilder.Sql(
                "UPDATE \"Processes\" SET \"ProcessCategoryId\" = \"Category\" + 1 WHERE \"IsDeleted\" = 0 OR \"IsDeleted\" = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ProcessCategoryId",
                table: "Processes",
                column: "ProcessCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Processes_ProcessCategories_ProcessCategoryId",
                table: "Processes",
                column: "ProcessCategoryId",
                principalTable: "ProcessCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Add RoomId column to Materials
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Materials",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_RoomId",
                table: "Materials",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Rooms_RoomId",
                table: "Materials",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Rooms_RoomId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_RoomId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Processes_ProcessCategories_ProcessCategoryId",
                table: "Processes");

            migrationBuilder.DropIndex(
                name: "IX_Processes_ProcessCategoryId",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "ProcessCategoryId",
                table: "Processes");

            migrationBuilder.DropTable(
                name: "ProcessCategories");
        }
    }
}
