using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class GamificationFitnessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserWorkout_UserId",
                table: "UserWorkout");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeParticipant_UserId",
                table: "ChallengeParticipant");

            migrationBuilder.AddColumn<int>(
                name: "AccumulatedPausedSeconds",
                table: "UserWorkout",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserWorkout",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAt",
                table: "UserWorkout",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "UserWorkout",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedExerciseId",
                table: "UserGoal",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedWorkoutType",
                table: "UserGoal",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "UserGoal",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "AchievedValue",
                table: "UserBadge",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceSummary",
                table: "UserBadge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceType",
                table: "UserBadge",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeaturedOrder",
                table: "UserBadge",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TriggeringEntityId",
                table: "UserBadge",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "GoalType",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "GoalType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GoalType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetricCode",
                table: "GoalType",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParameterKind",
                table: "GoalType",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "ChallengeParticipant",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "ChallengeParticipant",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ChallengeParticipant",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Challenge",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Challenge",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinClosing",
                table: "Challenge",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetricCode",
                table: "Challenge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "Challenge",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParticipantLimit",
                table: "Challenge",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Challenge",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TargetValue",
                table: "Challenge",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Challenge",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkoutType",
                table: "Challenge",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Badge",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Badge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CriterionCode",
                table: "Badge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Badge",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Badge",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetricCode",
                table: "Badge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriesCode",
                table: "Badge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Threshold",
                table: "Badge",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Tier",
                table: "Badge",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            // Backfill legacy rows before the new uniqueness constraints are created.
            migrationBuilder.Sql("UPDATE \"GoalType\" SET \"Code\" = 'legacy-' || \"Id\"::text, \"ParameterKind\" = 'None', \"IsActive\" = false WHERE \"Code\" = '';");
            migrationBuilder.Sql("UPDATE \"Badge\" SET \"Code\" = 'legacy-' || \"Id\"::text, \"Category\" = 'Legacy', \"CriterionCode\" = 'Legacy', \"Tier\" = 'None', \"IsActive\" = false WHERE \"Code\" = '';");
            migrationBuilder.Sql("UPDATE \"UserGoal\" SET \"TimeZoneId\" = 'UTC' WHERE \"TimeZoneId\" = '';");
            migrationBuilder.Sql("UPDATE \"Challenge\" SET \"MetricCode\" = 'workout.count', \"Mode\" = 'Target', \"TargetValue\" = 1, \"Visibility\" = 'Public', \"JoinClosing\" = 'AtStart', \"ParticipantLimit\" = 100, \"Status\" = CASE WHEN \"EndDate\" <= NOW() THEN 'Completed' WHEN \"StartDate\" <= NOW() THEN 'Active' ELSE 'Upcoming' END WHERE \"MetricCode\" = '';");
            migrationBuilder.Sql("UPDATE \"ChallengeParticipant\" SET \"Status\" = 'Active', \"JoinedAt\" = NOW() WHERE \"Status\" = ''; ");
            migrationBuilder.Sql("WITH ranked AS (SELECT \"Id\", ROW_NUMBER() OVER (PARTITION BY \"UserId\" ORDER BY \"CreatedAt\" DESC) AS rn FROM \"UserWorkout\" WHERE \"Status\" IN (0, 2)) UPDATE \"UserWorkout\" SET \"Status\" = 3, \"DeletedAt\" = NOW() WHERE \"Id\" IN (SELECT \"Id\" FROM ranked WHERE rn > 1);");

            migrationBuilder.CreateTable(
                name: "ActivityContribution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceWorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    WorkoutType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityContribution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeInvitation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeInvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeInvitation_Challenge_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeInvitation_User_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    IsFinisher = table.Column<bool>(type: "boolean", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeResult_Challenge_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeResult_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeScoreContribution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityContributionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeScoreContribution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoalPeriod",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetValue = table.Column<double>(type: "double precision", nullable: false),
                    ProgressValue = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalPeriod_UserGoal_GoalId",
                        column: x => x.GoalId,
                        principalTable: "UserGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetricDefinition",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CanonicalUnit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Aggregation = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    IsGoalSupported = table.Column<bool>(type: "boolean", nullable: false),
                    IsChallengeSupported = table.Column<bool>(type: "boolean", nullable: false),
                    IsBadgeSupported = table.Column<bool>(type: "boolean", nullable: false),
                    IsAnalyticsSupported = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricDefinition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkout_OneActiveSessionPerUser",
                table: "UserWorkout",
                column: "UserId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL AND \"Status\" IN (0, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadge_UserId_FeaturedOrder",
                table: "UserBadge",
                columns: new[] { "UserId", "FeaturedOrder" },
                unique: true,
                filter: "\"FeaturedOrder\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoalType_Code",
                table: "GoalType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipant_UserId_Status",
                table: "ChallengeParticipant",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Challenge_Status_StartDate_EndDate",
                table: "Challenge",
                columns: new[] { "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Badge_Code",
                table: "Badge",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContribution_SourceWorkoutId_MetricCode_ExerciseId",
                table: "ActivityContribution",
                columns: new[] { "SourceWorkoutId", "MetricCode", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContribution_UserId_MetricCode_OccurredAt",
                table: "ActivityContribution",
                columns: new[] { "UserId", "MetricCode", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContribution_UserId_WorkoutType_OccurredAt",
                table: "ActivityContribution",
                columns: new[] { "UserId", "WorkoutType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeInvitation_ChallengeId_InvitedUserId",
                table: "ChallengeInvitation",
                columns: new[] { "ChallengeId", "InvitedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeInvitation_InvitedUserId",
                table: "ChallengeInvitation",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeResult_ChallengeId_ParticipantId",
                table: "ChallengeResult",
                columns: new[] { "ChallengeId", "ParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeResult_UserId_FinalizedAt",
                table: "ChallengeResult",
                columns: new[] { "UserId", "FinalizedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeScoreContribution_ChallengeId",
                table: "ChallengeScoreContribution",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeScoreContribution_ParticipantId_ActivityContributi~",
                table: "ChallengeScoreContribution",
                columns: new[] { "ParticipantId", "ActivityContributionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalPeriod_GoalId_StartAt",
                table: "GoalPeriod",
                columns: new[] { "GoalId", "StartAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalPeriod_Status_EndAt",
                table: "GoalPeriod",
                columns: new[] { "Status", "EndAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityContribution");

            migrationBuilder.DropTable(
                name: "ChallengeInvitation");

            migrationBuilder.DropTable(
                name: "ChallengeResult");

            migrationBuilder.DropTable(
                name: "ChallengeScoreContribution");

            migrationBuilder.DropTable(
                name: "GoalPeriod");

            migrationBuilder.DropTable(
                name: "MetricDefinition");

            migrationBuilder.DropIndex(
                name: "IX_UserWorkout_OneActiveSessionPerUser",
                table: "UserWorkout");

            migrationBuilder.DropIndex(
                name: "IX_UserBadge_UserId_FeaturedOrder",
                table: "UserBadge");

            migrationBuilder.DropIndex(
                name: "IX_GoalType_Code",
                table: "GoalType");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeParticipant_UserId_Status",
                table: "ChallengeParticipant");

            migrationBuilder.DropIndex(
                name: "IX_Challenge_Status_StartDate_EndDate",
                table: "Challenge");

            migrationBuilder.DropIndex(
                name: "IX_Badge_Code",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "AccumulatedPausedSeconds",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "SelectedExerciseId",
                table: "UserGoal");

            migrationBuilder.DropColumn(
                name: "SelectedWorkoutType",
                table: "UserGoal");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "UserGoal");

            migrationBuilder.DropColumn(
                name: "AchievedValue",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "EvidenceSummary",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "EvidenceType",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "FeaturedOrder",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "TriggeringEntityId",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "MetricCode",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "ParameterKind",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "ChallengeParticipant");

            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "ChallengeParticipant");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChallengeParticipant");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "JoinClosing",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "MetricCode",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "ParticipantLimit",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "WorkoutType",
                table: "Challenge");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "CriterionCode",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "MetricCode",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "SeriesCode",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "Badge");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkout_UserId",
                table: "UserWorkout",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipant_UserId",
                table: "ChallengeParticipant",
                column: "UserId");
        }
    }
}
