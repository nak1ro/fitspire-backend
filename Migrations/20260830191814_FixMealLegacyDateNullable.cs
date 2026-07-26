using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixMealLegacyDateNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "LegacyDate" was left over from renaming Meal.Date -> Meal.MealDate in
            // AddNutritionTracking. Current code never writes to it (no CLR property maps
            // to it), but it was never relaxed from NOT NULL like its sibling
            // "LegacyTotalCalories" was in the same migration, so every meal insert fails.
            migrationBuilder.AlterColumn<DateTime>(
                name: "LegacyDate",
                table: "Meal",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LegacyDate",
                table: "Meal",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
