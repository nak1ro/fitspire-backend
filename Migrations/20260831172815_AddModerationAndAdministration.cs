using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationAndAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAtUtc",
                table: "User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedUntilUtc",
                table: "User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "User",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationRemovedAtUtc",
                table: "Post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationRemovedAtUtc",
                table: "MediaAsset",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationRemovedAtUtc",
                table: "Comment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModerationReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaContext = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Reason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResolutionOutcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TargetSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    SnapshotVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationReport_User_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationReport_User_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationReport_User_SubjectUserId",
                        column: x => x.SubjectUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModerationAction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeratorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SuspensionEndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationAction_ModerationReport_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ModerationReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationAction_User_ModeratorUserId",
                        column: x => x.ModeratorUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationAction_User_SubjectUserId",
                        column: x => x.SubjectUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAction_ModeratorUserId",
                table: "ModerationAction",
                column: "ModeratorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAction_ReportId_OccurredAtUtc",
                table: "ModerationAction",
                columns: new[] { "ReportId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAction_SubjectUserId",
                table: "ModerationAction",
                column: "SubjectUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReport_ResolvedByUserId",
                table: "ModerationReport",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReport_Status_CreatedAt",
                table: "ModerationReport",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReport_Status_TargetType_CreatedAt",
                table: "ModerationReport",
                columns: new[] { "Status", "TargetType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReport_SubjectUserId_CreatedAt",
                table: "ModerationReport",
                columns: new[] { "SubjectUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_ModerationReport_OpenReporterTarget",
                table: "ModerationReport",
                columns: new[] { "ReporterUserId", "TargetType", "TargetId" },
                unique: true,
                filter: "\"Status\" = 'Open'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationAction");

            migrationBuilder.DropTable(
                name: "ModerationReport");

            migrationBuilder.DropColumn(
                name: "SuspendedAtUtc",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SuspendedUntilUtc",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ModerationRemovedAtUtc",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "ModerationRemovedAtUtc",
                table: "MediaAsset");

            migrationBuilder.DropColumn(
                name: "ModerationRemovedAtUtc",
                table: "Comment");
        }
    }
}
