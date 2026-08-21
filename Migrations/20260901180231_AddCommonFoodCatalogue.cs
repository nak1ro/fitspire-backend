using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCommonFoodCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommonFood",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    QuantityUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomUnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CaloriesKcal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    CarbsGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonFood", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommonFood_Category_DisplayOrder",
                table: "CommonFood",
                columns: new[] { "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CommonFood_Name",
                table: "CommonFood",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "UX_CommonFood_Code",
                table: "CommonFood",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommonFood");
        }
    }
}
