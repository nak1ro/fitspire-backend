using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SeriesCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Tier = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CriterionCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Threshold = table.Column<double>(type: "double precision", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CanonicalUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ShowProgressWhenLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badge", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "CyclingUserWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: false),
                    ElevationGainMeters = table.Column<double>(type: "double precision", nullable: true),
                    MapData = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    IsIndoor = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyclingUserWorkouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoalType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    MeasurementType = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RelatedWorkoutType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetricCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ParameterKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalType", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exercise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercise_ExerciseCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ExerciseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "BodyCheckIn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WeightKg = table.Column<double>(type: "double precision", precision: 8, scale: 2, nullable: true),
                    BodyFatPercent = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    WaistCm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    ChestCm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    HipsCm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    ArmCm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    ThighCm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    WellbeingScore = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PhotoMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyCheckIn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    WorkoutType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TargetValue = table.Column<double>(type: "double precision", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    JoinClosing = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ParticipantLimit = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenge", x => x.Id);
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
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipant_Challenge_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenge",
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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    RootCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplyToCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationRemovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_ReplyToCommentId",
                        column: x => x.ReplyToCommentId,
                        principalTable: "Comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_RootCommentId",
                        column: x => x.RootCommentId,
                        principalTable: "Comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommentLike",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentLike_Comment_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comment",
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
                    RefreshCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCoachBriefing", x => x.Id);
                });

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
                });

            migrationBuilder.CreateTable(
                name: "Follower",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follower", x => x.Id);
                    table.CheckConstraint("CK_Follower_NotSelfFollow", "\"FollowerId\" <> \"FollowedId\"");
                });

            migrationBuilder.CreateTable(
                name: "FollowRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequest", x => x.Id);
                    table.CheckConstraint("CK_FollowRequest_NoSelf", "\"RequesterId\" <> \"AddresseeId\"");
                    table.CheckConstraint("CK_FollowRequest_Status", "\"Status\" IN ('Pending', 'Accepted', 'Rejected', 'Cancelled')");
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
                });

            migrationBuilder.CreateTable(
                name: "GoalProgressEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousValue = table.Column<double>(type: "double precision", nullable: false),
                    NewValue = table.Column<double>(type: "double precision", nullable: false),
                    Delta = table.Column<double>(type: "double precision", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalProgressEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoalTargetChange",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousTargetValue = table.Column<double>(type: "double precision", nullable: false),
                    NewTargetValue = table.Column<double>(type: "double precision", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalTargetChange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GymWorkoutDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SplitType = table.Column<string>(type: "text", nullable: true),
                    IntensityLevel = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymWorkoutDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GymWorkoutExercise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GymWorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymWorkoutExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymWorkoutExercise_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GymWorkoutExercise_GymWorkoutDetails_GymWorkoutId",
                        column: x => x.GymWorkoutId,
                        principalTable: "GymWorkoutDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GymWorkoutSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GymWorkoutExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<double>(type: "double precision", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    IsWarmup = table.Column<bool>(type: "boolean", nullable: false),
                    Rpe = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymWorkoutSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymWorkoutSet_GymWorkoutExercise_GymWorkoutExerciseId",
                        column: x => x.GymWorkoutExerciseId,
                        principalTable: "GymWorkoutExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Meal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConsumedAtLocalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    MealType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    QuantityUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomUnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CaloriesKcal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    CarbsGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    FavouriteFoodId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealItem_FavouriteFood_FavouriteFoodId",
                        column: x => x.FavouriteFoodId,
                        principalTable: "FavouriteFood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealItem_Meal_MealId",
                        column: x => x.MealId,
                        principalTable: "Meal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaAsset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClientRequestId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DeclaredContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeclaredSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StagingBlobKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedETag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActualSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadUrlExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PendingExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingStartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttachedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CleanupAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextCleanupAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModerationRemovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAsset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaVariant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    BlobKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaVariant_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ProfilePictureMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FavoriteSport = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FitnessLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HeightCm = table.Column<double>(type: "double precision", nullable: true),
                    SuspendedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_MediaAsset_ProfilePictureMediaId",
                        column: x => x.ProfilePictureMediaId,
                        principalTable: "MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_User_UserId",
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

            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationRemovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceWorkoutId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWorkoutType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SharedWorkoutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SharedDurationMinutes = table.Column<double>(type: "double precision", nullable: true),
                    SharedDistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    SharedCaloriesBurned = table.Column<int>(type: "integer", nullable: true),
                    SharedTotalVolumeKg = table.Column<double>(type: "double precision", nullable: true),
                    SharedExerciseCount = table.Column<int>(type: "integer", nullable: true),
                    SharedCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceGoalId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedGoalTypeName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SharedGoalTargetValue = table.Column<double>(type: "double precision", nullable: true),
                    SharedGoalUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SharedGoalCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourcePersonalRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedPersonalRecordWorkoutType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SharedPersonalRecordMetric = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SharedPersonalRecordExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedPersonalRecordExerciseName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SharedPersonalRecordValue = table.Column<double>(type: "double precision", nullable: true),
                    SharedPersonalRecordUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SharedPersonalRecordAchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBadge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    AchievedValue = table.Column<double>(type: "double precision", nullable: true),
                    EvidenceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ThresholdSnapshot = table.Column<double>(type: "double precision", nullable: true),
                    CanonicalUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TriggeringEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TriggeringEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FeaturedOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadge_Badge_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBadge_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGoal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetValue = table.Column<double>(type: "double precision", nullable: false),
                    CurrentValue = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    LastStreakDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SelectedWorkoutType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SelectedExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefinitionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGoal_GoalType_GoalTypeId",
                        column: x => x.GoalTypeId,
                        principalTable: "GoalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGoal_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                    IsDarkModeEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReceiveEmailNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UnitSystem = table.Column<string>(type: "text", nullable: false, defaultValue: "metric"),
                    TimeZoneId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreference_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWorkout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutType = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PausedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccumulatedPausedSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CaloriesBurned = table.Column<int>(type: "integer", nullable: true),
                    CreatedFromRoutineId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkout_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "WorkoutRoutines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkoutType = table.Column<string>(type: "text", nullable: false),
                    RoutineDataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutRoutines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutRoutines_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "PostLike",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostLike_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLike_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostMedia_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostMedia_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedPost",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedPost_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedPost_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutType = table.Column<string>(type: "text", nullable: false),
                    Metric = table.Column<string>(type: "text", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalRecord_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonalRecord_UserWorkout_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonalRecord_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalRecordHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutType = table.Column<string>(type: "text", nullable: false),
                    Metric = table.Column<string>(type: "text", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalRecordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalRecordHistory_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonalRecordHistory_UserWorkout_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PersonalRecordHistory_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RunningUserWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: false),
                    ElevationGainMeters = table.Column<double>(type: "double precision", nullable: true),
                    StepCount = table.Column<int>(type: "integer", nullable: true),
                    MapData = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunningUserWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunningUserWorkouts_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwimmingUserWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Laps = table.Column<int>(type: "integer", nullable: true),
                    PoolLengthMeters = table.Column<double>(type: "double precision", nullable: true),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    StrokeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwimmingUserWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwimmingUserWorkouts_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YogaUserWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Style = table.Column<string>(type: "text", nullable: true),
                    Intensity = table.Column<string>(type: "text", nullable: true),
                    FocusArea = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YogaUserWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YogaUserWorkouts_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Badge_Code",
                table: "Badge",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BodyCheckIn_PhotoMediaId",
                table: "BodyCheckIn",
                column: "PhotoMediaId",
                unique: true,
                filter: "\"PhotoMediaId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_BodyCheckIn_ActiveUserDate",
                table: "BodyCheckIn",
                columns: new[] { "UserId", "CheckInDate" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Challenge_CreatedBy",
                table: "Challenge",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Challenge_Status_StartDate_EndDate",
                table: "Challenge",
                columns: new[] { "Status", "StartDate", "EndDate" });

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
                name: "IX_ChallengeParticipant_ChallengeId_UserId",
                table: "ChallengeParticipant",
                columns: new[] { "ChallengeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipant_UserId_Status",
                table: "ChallengeParticipant",
                columns: new[] { "UserId", "Status" });

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
                name: "IX_Comment_PostId_RootCommentId_CreatedAt",
                table: "Comment",
                columns: new[] { "PostId", "RootCommentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ReplyToCommentId",
                table: "Comment",
                column: "ReplyToCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_RootCommentId",
                table: "Comment",
                column: "RootCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_UserId",
                table: "Comment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLike_CommentId",
                table: "CommentLike",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLike_UserId_CommentId",
                table: "CommentLike",
                columns: new[] { "UserId", "CommentId" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_DailyCoachBriefing_Status_RequestedAt",
                table: "DailyCoachBriefing",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_DailyCoachBriefing_UserId_LocalDate",
                table: "DailyCoachBriefing",
                columns: new[] { "UserId", "LocalDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_CategoryId",
                table: "Exercise",
                column: "CategoryId");

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
                name: "IX_Follower_FollowedId",
                table: "Follower",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_Follower_FollowerId_FollowedId",
                table: "Follower",
                columns: new[] { "FollowerId", "FollowedId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequest_AddresseeId",
                table: "FollowRequest",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequest_RequesterId_AddresseeId",
                table: "FollowRequest",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true,
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_GoalPeriod_GoalId_StartAt",
                table: "GoalPeriod",
                columns: new[] { "GoalId", "StartAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalPeriod_Status_EndAt",
                table: "GoalPeriod",
                columns: new[] { "Status", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalProgressEntry_GoalId",
                table: "GoalProgressEntry",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTargetChange_GoalId_ChangedAt",
                table: "GoalTargetChange",
                columns: new[] { "GoalId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalType_Code",
                table: "GoalType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymWorkoutExercise_ExerciseId",
                table: "GymWorkoutExercise",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_GymWorkoutExercise_GymWorkoutId",
                table: "GymWorkoutExercise",
                column: "GymWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_GymWorkoutSet_GymWorkoutExerciseId_OrderIndex",
                table: "GymWorkoutSet",
                columns: new[] { "GymWorkoutExerciseId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meal_UserId_MealDate",
                table: "Meal",
                columns: new[] { "UserId", "MealDate" });

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
                name: "IX_MediaAsset_OwnerUserId_ClientRequestId",
                table: "MediaAsset",
                columns: new[] { "OwnerUserId", "ClientRequestId" },
                unique: true,
                filter: "\"ClientRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAsset_OwnerUserId_Status_PendingExpiresAtUtc",
                table: "MediaAsset",
                columns: new[] { "OwnerUserId", "Status", "PendingExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaAsset_StagingBlobKey",
                table: "MediaAsset",
                column: "StagingBlobKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAsset_Status_NextCleanupAttemptAtUtc",
                table: "MediaAsset",
                columns: new[] { "Status", "NextCleanupAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaVariant_BlobKey",
                table: "MediaVariant",
                column: "BlobKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaVariant_MediaAssetId_Kind",
                table: "MediaVariant",
                columns: new[] { "MediaAssetId", "Kind" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notification",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "Notification",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "UX_NutritionTarget_UserId",
                table: "NutritionTarget",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_ExerciseId",
                table: "PersonalRecord",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_UserId_Featured",
                table: "PersonalRecord",
                column: "UserId",
                unique: true,
                filter: "\"IsFeatured\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric",
                table: "PersonalRecord",
                columns: new[] { "UserId", "WorkoutType", "Metric" },
                unique: true,
                filter: "\"ExerciseId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric_ExerciseId",
                table: "PersonalRecord",
                columns: new[] { "UserId", "WorkoutType", "Metric", "ExerciseId" },
                unique: true,
                filter: "\"ExerciseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_WorkoutId",
                table: "PersonalRecord",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecordHistory_ExerciseId",
                table: "PersonalRecordHistory",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecordHistory_UserId",
                table: "PersonalRecordHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecordHistory_WorkoutId",
                table: "PersonalRecordHistory",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourceGoalId",
                table: "Post",
                column: "SourceGoalId",
                unique: true,
                filter: "\"SourceGoalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourcePersonalRecordId_SharedPersonalRecordAchievedAt",
                table: "Post",
                columns: new[] { "SourcePersonalRecordId", "SharedPersonalRecordAchievedAt" },
                unique: true,
                filter: "\"SourcePersonalRecordId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourceWorkoutId",
                table: "Post",
                column: "SourceWorkoutId",
                unique: true,
                filter: "\"SourceWorkoutId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Post_UserId_CreatedAt",
                table: "Post",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_PostId",
                table: "PostLike",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_UserId_PostId",
                table: "PostLike",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_MediaAssetId",
                table: "PostMedia",
                column: "MediaAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_PostId_Order",
                table: "PostMedia",
                columns: new[] { "PostId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedPost_PostId",
                table: "SavedPost",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPost_UserId_PostId",
                table: "SavedPost",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_User_IsPrivate_FavoriteSport",
                table: "User",
                columns: new[] { "IsPrivate", "FavoriteSport" });

            migrationBuilder.CreateIndex(
                name: "IX_User_ProfilePictureMediaId",
                table: "User",
                column: "ProfilePictureMediaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "User",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBadge_BadgeId",
                table: "UserBadge",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadge_UserId_BadgeId",
                table: "UserBadge",
                columns: new[] { "UserId", "BadgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBadge_UserId_FeaturedOrder",
                table: "UserBadge",
                columns: new[] { "UserId", "FeaturedOrder" },
                unique: true,
                filter: "\"FeaturedOrder\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoal_GoalTypeId",
                table: "UserGoal",
                column: "GoalTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_UserGoal_ActiveDefinition",
                table: "UserGoal",
                columns: new[] { "UserId", "DefinitionKey" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreference_UserId",
                table: "UserPreference",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkout_OneActiveSessionPerUser",
                table: "UserWorkout",
                column: "UserId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL AND \"Status\" IN (0, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyCoachReport_Status_RequestedAt",
                table: "WeeklyCoachReport",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_WeeklyCoachReport_UserId_PeriodStart",
                table: "WeeklyCoachReport",
                columns: new[] { "UserId", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutines_UserId",
                table: "WorkoutRoutines",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_User_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_User_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_User_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_User_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BodyCheckIn_MediaAsset_PhotoMediaId",
                table: "BodyCheckIn",
                column: "PhotoMediaId",
                principalTable: "MediaAsset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BodyCheckIn_User_UserId",
                table: "BodyCheckIn",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenge_User_CreatedBy",
                table: "Challenge",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeInvitation_User_InvitedUserId",
                table: "ChallengeInvitation",
                column: "InvitedUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeParticipant_User_UserId",
                table: "ChallengeParticipant",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeResult_User_UserId",
                table: "ChallengeResult",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachMessage_CoachThread_ThreadId",
                table: "CoachMessage",
                column: "ThreadId",
                principalTable: "CoachThread",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachMessage_User_UserId",
                table: "CoachMessage",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachThread_User_UserId",
                table: "CoachThread",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_User_UserId",
                table: "Comment",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentLike_User_UserId",
                table: "CommentLike",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyCoachBriefing_User_UserId",
                table: "DailyCoachBriefing",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteFood_User_UserId",
                table: "FavouriteFood",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Follower_User_FollowedId",
                table: "Follower",
                column: "FollowedId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Follower_User_FollowerId",
                table: "Follower",
                column: "FollowerId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequest_User_AddresseeId",
                table: "FollowRequest",
                column: "AddresseeId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequest_User_RequesterId",
                table: "FollowRequest",
                column: "RequesterId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalPeriod_UserGoal_GoalId",
                table: "GoalPeriod",
                column: "GoalId",
                principalTable: "UserGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalProgressEntry_UserGoal_GoalId",
                table: "GoalProgressEntry",
                column: "GoalId",
                principalTable: "UserGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalTargetChange_UserGoal_GoalId",
                table: "GoalTargetChange",
                column: "GoalId",
                principalTable: "UserGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GymWorkoutDetails_UserWorkout_Id",
                table: "GymWorkoutDetails",
                column: "Id",
                principalTable: "UserWorkout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Meal_User_UserId",
                table: "Meal",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAsset_User_OwnerUserId",
                table: "MediaAsset",
                column: "OwnerUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAsset_User_OwnerUserId",
                table: "MediaAsset");

            migrationBuilder.DropTable(
                name: "ActivityContribution");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BodyCheckIn");

            migrationBuilder.DropTable(
                name: "ChallengeInvitation");

            migrationBuilder.DropTable(
                name: "ChallengeParticipant");

            migrationBuilder.DropTable(
                name: "ChallengeResult");

            migrationBuilder.DropTable(
                name: "ChallengeScoreContribution");

            migrationBuilder.DropTable(
                name: "CoachMessage");

            migrationBuilder.DropTable(
                name: "CommentLike");

            migrationBuilder.DropTable(
                name: "CommonFood");

            migrationBuilder.DropTable(
                name: "CyclingUserWorkouts");

            migrationBuilder.DropTable(
                name: "DailyCoachBriefing");

            migrationBuilder.DropTable(
                name: "Follower");

            migrationBuilder.DropTable(
                name: "FollowRequest");

            migrationBuilder.DropTable(
                name: "GoalPeriod");

            migrationBuilder.DropTable(
                name: "GoalProgressEntry");

            migrationBuilder.DropTable(
                name: "GoalTargetChange");

            migrationBuilder.DropTable(
                name: "GymWorkoutSet");

            migrationBuilder.DropTable(
                name: "MealItem");

            migrationBuilder.DropTable(
                name: "MediaVariant");

            migrationBuilder.DropTable(
                name: "MetricDefinition");

            migrationBuilder.DropTable(
                name: "ModerationAction");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "NutritionTarget");

            migrationBuilder.DropTable(
                name: "PersonalRecord");

            migrationBuilder.DropTable(
                name: "PersonalRecordHistory");

            migrationBuilder.DropTable(
                name: "PostLike");

            migrationBuilder.DropTable(
                name: "PostMedia");

            migrationBuilder.DropTable(
                name: "RunningUserWorkouts");

            migrationBuilder.DropTable(
                name: "SavedPost");

            migrationBuilder.DropTable(
                name: "SwimmingUserWorkouts");

            migrationBuilder.DropTable(
                name: "UserBadge");

            migrationBuilder.DropTable(
                name: "UserPreference");

            migrationBuilder.DropTable(
                name: "WeeklyCoachReport");

            migrationBuilder.DropTable(
                name: "WorkoutRoutines");

            migrationBuilder.DropTable(
                name: "YogaUserWorkouts");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Challenge");

            migrationBuilder.DropTable(
                name: "CoachThread");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "UserGoal");

            migrationBuilder.DropTable(
                name: "GymWorkoutExercise");

            migrationBuilder.DropTable(
                name: "FavouriteFood");

            migrationBuilder.DropTable(
                name: "Meal");

            migrationBuilder.DropTable(
                name: "ModerationReport");

            migrationBuilder.DropTable(
                name: "Badge");

            migrationBuilder.DropTable(
                name: "Post");

            migrationBuilder.DropTable(
                name: "GoalType");

            migrationBuilder.DropTable(
                name: "Exercise");

            migrationBuilder.DropTable(
                name: "GymWorkoutDetails");

            migrationBuilder.DropTable(
                name: "ExerciseCategory");

            migrationBuilder.DropTable(
                name: "UserWorkout");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "MediaAsset");
        }
    }
}
