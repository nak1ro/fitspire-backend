using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAiWeeklyCoaching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyCoachReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GenerationAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationCount = table.Column<int>(type: "integer", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SnapshotSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    PromptVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ResponseSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ReportJson = table.Column<string>(type: "jsonb", nullable: true),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProviderResponseId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InputTokens = table.Column<int>(type: "integer", nullable: true),
                    OutputTokens = table.Column<int>(type: "integer", nullable: true),
                    TotalTokens = table.Column<int>(type: "integer", nullable: true),
                    LastFailureKind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastFailureMessage = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingLeaseExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyCoachReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyCoachReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyCoachReport_Status_RequestedAt",
                table: "WeeklyCoachReport",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_WeeklyCoachReport_UserId_PeriodStart",
                table: "WeeklyCoachReport",
                columns: new[] { "UserId", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyCoachReport");
        }
    }
}
