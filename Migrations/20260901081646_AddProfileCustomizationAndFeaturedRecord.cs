using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileCustomizationAndFeaturedRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FavoriteSport",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FitnessLevel",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HeightCm",
                table: "User",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "PersonalRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_UserId_Featured",
                table: "PersonalRecord",
                column: "UserId",
                unique: true,
                filter: "\"IsFeatured\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalRecord_UserId_Featured",
                table: "PersonalRecord");

            migrationBuilder.DropColumn(
                name: "FavoriteSport",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FitnessLevel",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PersonalRecord");
        }
    }
}
