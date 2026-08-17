using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalRecordAchievedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AchievedAt",
                table: "PersonalRecord",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill existing rows with the occurrence date of the workout that earned the
            // record, instead of leaving them at the column's placeholder default — this is the
            // same value the application will compute going forward.
            migrationBuilder.Sql(
                """
                UPDATE "PersonalRecord" pr
                SET "AchievedAt" = COALESCE(w."Date", pr."UpdatedAt", pr."CreatedAt")
                FROM "UserWorkout" w
                WHERE w."Id" = pr."WorkoutId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievedAt",
                table: "PersonalRecord");
        }
    }
}
