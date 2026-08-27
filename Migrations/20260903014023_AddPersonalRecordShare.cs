using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalRecordShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Post_UserId",
                table: "Post");

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedPersonalRecordAchievedAt",
                table: "Post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SharedPersonalRecordExerciseId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedPersonalRecordExerciseName",
                table: "Post",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedPersonalRecordMetric",
                table: "Post",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedPersonalRecordUnit",
                table: "Post",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SharedPersonalRecordValue",
                table: "Post",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedPersonalRecordWorkoutType",
                table: "Post",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourcePersonalRecordId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefreshCount",
                table: "DailyCoachBriefing",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsPrivate_FavoriteSport",
                table: "User",
                columns: new[] { "IsPrivate", "FavoriteSport" });

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourcePersonalRecordId_SharedPersonalRecordAchievedAt",
                table: "Post",
                columns: new[] { "SourcePersonalRecordId", "SharedPersonalRecordAchievedAt" },
                unique: true,
                filter: "\"SourcePersonalRecordId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_UserId_CreatedAt",
                table: "Post",
                columns: new[] { "UserId", "CreatedAt" });

            // Restored from an orphaned migration another agent created but never applied
            // (missing its Designer.cs, never in __EFMigrationsHistory) — the trigram search
            // indexes it added are real, deliberate work, not something to silently drop.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE INDEX \"IX_User_DisplayName_trgm\" ON \"User\" USING gin (\"DisplayName\" gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX \"IX_User_UserName_trgm\" ON \"User\" USING gin (\"UserName\" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_User_UserName_trgm\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_User_DisplayName_trgm\";");

            migrationBuilder.DropIndex(
                name: "IX_User_IsPrivate_FavoriteSport",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Post_SourcePersonalRecordId_SharedPersonalRecordAchievedAt",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Post_UserId_CreatedAt",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordAchievedAt",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordExerciseId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordExerciseName",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordMetric",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordUnit",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordValue",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedPersonalRecordWorkoutType",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SourcePersonalRecordId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "RefreshCount",
                table: "DailyCoachBriefing");

            migrationBuilder.CreateIndex(
                name: "IX_Post_UserId",
                table: "Post",
                column: "UserId");
        }
    }
}
