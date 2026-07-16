using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealItem_MealId",
                table: "MealItem");

            migrationBuilder.DropIndex(
                name: "IX_Meal_UserId",
                table: "Meal");

            migrationBuilder.RenameColumn(name: "Calories", table: "MealItem", newName: "LegacyCalories");
            migrationBuilder.RenameColumn(name: "Carbs", table: "MealItem", newName: "LegacyCarbs");
            migrationBuilder.RenameColumn(name: "Fat", table: "MealItem", newName: "LegacyFat");
            migrationBuilder.RenameColumn(name: "Protein", table: "MealItem", newName: "LegacyProtein");
            migrationBuilder.RenameColumn(name: "Unit", table: "MealItem", newName: "LegacyUnit");
            migrationBuilder.RenameColumn(name: "Date", table: "Meal", newName: "LegacyDate");
            migrationBuilder.RenameColumn(name: "TotalCalories", table: "Meal", newName: "LegacyTotalCalories");

            migrationBuilder.Sql("""
                UPDATE "MealItem"
                SET "Name" = COALESCE(NULLIF(LEFT(BTRIM("Name"), 200), ''), 'Legacy food'),
                    "Quantity" = CASE WHEN "Quantity" IS NULL OR "Quantity" <= 0 THEN 1 ELSE "Quantity" END;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "MealItem",
                type: "numeric(12,3)",
                precision: 12,
                scale: 3,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MealItem",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "CaloriesKcal",
                table: "MealItem",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarbsGrams",
                table: "MealItem",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MealItem",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "CustomUnitName",
                table: "MealItem",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MealItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FatGrams",
                table: "MealItem",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FavouriteFoodId",
                table: "MealItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "MealItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinGrams",
                table: "MealItem",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnit",
                table: "MealItem",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CustomServing");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotKey",
                table: "MealItem",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                defaultValue: "legacy");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MealItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ConsumedAtLocalTime",
                table: "Meal",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Meal",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MealDate",
                table: "Meal",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "MealType",
                table: "Meal",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Snack");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Meal",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Meal"
                SET "MealDate" = ("LegacyDate" AT TIME ZONE 'UTC')::date,
                    "MealType" = 'Snack';

                UPDATE "MealItem"
                SET "CustomUnitName" = COALESCE(NULLIF(LEFT(BTRIM("LegacyUnit"), 50), ''), 'serving'),
                    "QuantityUnit" = 'CustomServing',
                    "CaloriesKcal" = CASE WHEN "LegacyCalories" >= 0 THEN "LegacyCalories"::numeric(12,2) END,
                    "ProteinGrams" = CASE WHEN "LegacyProtein" >= 0 THEN "LegacyProtein"::numeric(12,2) END,
                    "CarbsGrams" = CASE WHEN "LegacyCarbs" >= 0 THEN "LegacyCarbs"::numeric(12,2) END,
                    "FatGrams" = CASE WHEN "LegacyFat" >= 0 THEN "LegacyFat"::numeric(12,2) END;

                WITH ordered_items AS (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "MealId" ORDER BY "Id") AS order_index
                    FROM "MealItem"
                )
                UPDATE "MealItem" AS item
                SET "OrderIndex" = ordered_items.order_index
                FROM ordered_items
                WHERE item."Id" = ordered_items."Id";

                UPDATE "MealItem"
                SET "SnapshotKey" = UPPER(BTRIM("Name")) || '|' || "Quantity"::text || '|CustomServing|' ||
                    UPPER(BTRIM("CustomUnitName")) || '|' || COALESCE("CaloriesKcal"::text, '') || '|' ||
                    COALESCE("ProteinGrams"::text, '') || '|' || COALESCE("CarbsGrams"::text, '') || '|' ||
                    COALESCE("FatGrams"::text, '');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "OrderIndex",
                table: "MealItem",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "QuantityUnit",
                table: "MealItem",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "CustomServing");

            migrationBuilder.AlterColumn<string>(
                name: "SnapshotKey",
                table: "MealItem",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldDefaultValue: "legacy");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "MealDate",
                table: "Meal",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<string>(
                name: "MealType",
                table: "Meal",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Snack");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Meal",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Meal",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FavouriteFood",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefinitionKey = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    QuantityUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomUnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CaloriesKcal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    CarbsGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteFood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavouriteFood_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionTarget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaloriesKcal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    CarbsGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionTarget_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealItem_FavouriteFoodId",
                table: "MealItem",
                column: "FavouriteFoodId");

            migrationBuilder.CreateIndex(
                name: "UX_MealItem_ActiveMealOrder",
                table: "MealItem",
                columns: new[] { "MealId", "OrderIndex" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Meal_UserId_MealDate",
                table: "Meal",
                columns: new[] { "UserId", "MealDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteFood_UserId_Name",
                table: "FavouriteFood",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "UX_FavouriteFood_ActiveUserDefinition",
                table: "FavouriteFood",
                columns: new[] { "UserId", "DefinitionKey" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_NutritionTarget_UserId",
                table: "NutritionTarget",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MealItem_FavouriteFood_FavouriteFoodId",
                table: "MealItem",
                column: "FavouriteFoodId",
                principalTable: "FavouriteFood",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealItem_FavouriteFood_FavouriteFoodId",
                table: "MealItem");

            migrationBuilder.DropTable(
                name: "FavouriteFood");

            migrationBuilder.DropTable(
                name: "NutritionTarget");

            migrationBuilder.DropIndex(
                name: "IX_MealItem_FavouriteFoodId",
                table: "MealItem");

            migrationBuilder.DropIndex(
                name: "UX_MealItem_ActiveMealOrder",
                table: "MealItem");

            migrationBuilder.DropIndex(
                name: "IX_Meal_UserId_MealDate",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "CaloriesKcal",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "CarbsGrams",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "CustomUnitName",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "FatGrams",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "FavouriteFoodId",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "ProteinGrams",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "SnapshotKey",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MealItem");

            migrationBuilder.DropColumn(
                name: "ConsumedAtLocalTime",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "MealDate",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "MealType",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Meal");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Meal");

            migrationBuilder.RenameColumn(name: "LegacyCalories", table: "MealItem", newName: "Calories");
            migrationBuilder.RenameColumn(name: "LegacyCarbs", table: "MealItem", newName: "Carbs");
            migrationBuilder.RenameColumn(name: "LegacyFat", table: "MealItem", newName: "Fat");
            migrationBuilder.RenameColumn(name: "LegacyProtein", table: "MealItem", newName: "Protein");
            migrationBuilder.RenameColumn(name: "LegacyUnit", table: "MealItem", newName: "Unit");
            migrationBuilder.RenameColumn(name: "LegacyDate", table: "Meal", newName: "Date");
            migrationBuilder.RenameColumn(name: "LegacyTotalCalories", table: "Meal", newName: "TotalCalories");

            migrationBuilder.AlterColumn<float>(
                name: "Quantity",
                table: "MealItem",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,3)",
                oldPrecision: 12,
                oldScale: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MealItem",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_MealItem_MealId",
                table: "MealItem",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_Meal_UserId",
                table: "Meal",
                column: "UserId");
        }
    }
}
