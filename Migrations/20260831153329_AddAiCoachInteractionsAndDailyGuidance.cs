using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAiCoachInteractionsAndDailyGuidance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoachThread",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HasCustomTitle = table.Column<bool>(type: "boolean", nullable: false),
                    ContextSummary = table.Column<string>(type: "character varying(1400)", maxLength: 1400, nullable: true),
                    NextSequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    LastSummarySequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachThread", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachThread_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyCoachBriefing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GenerationAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SnapshotSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    PromptVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ResponseSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ContentJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_DailyCoachBriefing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyCoachBriefing_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Question = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AnswerJson = table.Column<string>(type: "jsonb", nullable: true),
                    LocalRequestDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GenerationAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SnapshotSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ContextSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    PromptVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ResponseSchemaVersion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("PK_CoachMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachMessage_CoachThread_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "CoachThread",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachMessage_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachMessage_Status_RequestedAt",
                table: "CoachMessage",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachMessage_UserId_LocalRequestDate",
                table: "CoachMessage",
                columns: new[] { "UserId", "LocalRequestDate" });

            migrationBuilder.CreateIndex(
                name: "UX_CoachMessage_ThreadId_SequenceNumber",
                table: "CoachMessage",
                columns: new[] { "ThreadId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CoachMessage_UserId_ClientRequestId",
                table: "CoachMessage",
                columns: new[] { "UserId", "ClientRequestId" },
                unique: true,
                filter: "\"ClientRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CoachThread_UserId_LastActivityAt",
                table: "CoachThread",
                columns: new[] { "UserId", "LastActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyCoachBriefing_Status_RequestedAt",
                table: "DailyCoachBriefing",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_DailyCoachBriefing_UserId_LocalDate",
                table: "DailyCoachBriefing",
                columns: new[] { "UserId", "LocalDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachMessage");

            migrationBuilder.DropTable(
                name: "DailyCoachBriefing");

            migrationBuilder.DropTable(
                name: "CoachThread");
        }
    }
}
