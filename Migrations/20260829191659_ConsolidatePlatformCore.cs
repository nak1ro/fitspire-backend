using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatePlatformCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBadge_Badge_BadgeId",
                table: "UserBadge");

            migrationBuilder.DropIndex(
                name: "IX_UserGoal_UserId",
                table: "UserGoal");

            migrationBuilder.DropIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric",
                table: "PersonalRecord");

            migrationBuilder.Sql("""
                DO $EF$
                BEGIN
                    RAISE NOTICE 'Removing % friendship rows and % friendship-request rows without converting them to follows.',
                        (SELECT COUNT(*) FROM "Friendship"),
                        (SELECT COUNT(*) FROM "FriendshipRequest");
                END $EF$;
                """);

            migrationBuilder.DropTable(
                name: "Friendship");

            migrationBuilder.DropTable(
                name: "FriendshipRequest");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureUrl",
                table: "User",
                newName: "LegacyProfilePictureUrl");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Post",
                newName: "LegacyImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "DefinitionKey",
                table: "UserGoal",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "UserBadge",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanonicalUnit",
                table: "UserBadge",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ThresholdSnapshot",
                table: "UserBadge",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TriggeringEntityType",
                table: "UserBadge",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePictureMediaId",
                table: "User",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "PersonalRecordHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "PersonalRecord",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "GymWorkoutExercise",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Badge",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Badge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Badge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanonicalUnit",
                table: "Badge",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowProgressWhenLocked",
                table: "Badge",
                type: "boolean",
                nullable: false,
                defaultValue: true);

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
                    table.ForeignKey(
                        name: "FK_GoalTargetChange_UserGoal_GoalId",
                        column: x => x.GoalId,
                        principalTable: "UserGoal",
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAsset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAsset_User_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.Sql("""
                INSERT INTO "GymWorkoutSet" (
                    "Id", "GymWorkoutExerciseId", "OrderIndex", "Reps", "WeightKg",
                    "IsWarmup", "IsCompleted", "CompletedAtUtc", "CreatedAt", "UpdatedAt")
                SELECT
                    (substring(md5(legacy."Id"::text || ':' || generated."SetIndex"::text), 1, 8) || '-' ||
                     substring(md5(legacy."Id"::text || ':' || generated."SetIndex"::text), 9, 4) || '-' ||
                     substring(md5(legacy."Id"::text || ':' || generated."SetIndex"::text), 13, 4) || '-' ||
                     substring(md5(legacy."Id"::text || ':' || generated."SetIndex"::text), 17, 4) || '-' ||
                     substring(md5(legacy."Id"::text || ':' || generated."SetIndex"::text), 21, 12))::uuid,
                    legacy."Id",
                    generated."SetIndex",
                    NULLIF(legacy."Reps", 0),
                    NULLIF(legacy."Weight", 0),
                    FALSE,
                    workout."Status" = 'Completed' AND legacy."Reps" > 0,
                    CASE WHEN workout."Status" = 'Completed' AND legacy."Reps" > 0
                        THEN COALESCE(workout."CompletedAt", legacy."UpdatedAt", legacy."CreatedAt")
                    END,
                    legacy."CreatedAt",
                    legacy."UpdatedAt"
                FROM "GymWorkoutExercise" AS legacy
                INNER JOIN "UserWorkout" AS workout ON workout."Id" = legacy."GymWorkoutId"
                CROSS JOIN LATERAL generate_series(1, GREATEST(legacy."Sets", 0)) AS generated("SetIndex");
                """);

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "Reps",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "Sets",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "GymWorkoutExercise");

            migrationBuilder.CreateIndex(
                name: "UX_UserGoal_ActiveDefinition",
                table: "UserGoal",
                columns: new[] { "UserId", "DefinitionKey" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_User_ProfilePictureMediaId",
                table: "User",
                column: "ProfilePictureMediaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecordHistory_ExerciseId",
                table: "PersonalRecordHistory",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_ExerciseId",
                table: "PersonalRecord",
                column: "ExerciseId");

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
                name: "IX_GoalTargetChange_GoalId_ChangedAt",
                table: "GoalTargetChange",
                columns: new[] { "GoalId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GymWorkoutSet_GymWorkoutExerciseId_OrderIndex",
                table: "GymWorkoutSet",
                columns: new[] { "GymWorkoutExerciseId", "OrderIndex" },
                unique: true);

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
                name: "IX_PostMedia_MediaAssetId",
                table: "PostMedia",
                column: "MediaAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_PostId_Order",
                table: "PostMedia",
                columns: new[] { "PostId", "Order" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalRecord_Exercise_ExerciseId",
                table: "PersonalRecord",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalRecordHistory_Exercise_ExerciseId",
                table: "PersonalRecordHistory",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_MediaAsset_ProfilePictureMediaId",
                table: "User",
                column: "ProfilePictureMediaId",
                principalTable: "MediaAsset",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBadge_Badge_BadgeId",
                table: "UserBadge",
                column: "BadgeId",
                principalTable: "Badge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalRecord_Exercise_ExerciseId",
                table: "PersonalRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalRecordHistory_Exercise_ExerciseId",
                table: "PersonalRecordHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_User_MediaAsset_ProfilePictureMediaId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBadge_Badge_BadgeId",
                table: "UserBadge");

            migrationBuilder.DropTable(
                name: "GoalTargetChange");

            migrationBuilder.DropTable(
                name: "GymWorkoutSet");

            migrationBuilder.DropTable(
                name: "MediaVariant");

            migrationBuilder.DropTable(
                name: "PostMedia");

            migrationBuilder.DropTable(
                name: "MediaAsset");

            migrationBuilder.DropIndex(
                name: "UX_UserGoal_ActiveDefinition",
                table: "UserGoal");

            migrationBuilder.DropIndex(
                name: "IX_User_ProfilePictureMediaId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_PersonalRecordHistory_ExerciseId",
                table: "PersonalRecordHistory");

            migrationBuilder.DropIndex(
                name: "IX_PersonalRecord_ExerciseId",
                table: "PersonalRecord");

            migrationBuilder.DropIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric",
                table: "PersonalRecord");

            migrationBuilder.DropIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric_ExerciseId",
                table: "PersonalRecord");

            migrationBuilder.DropColumn(
                name: "DefinitionKey",
                table: "UserGoal");

            migrationBuilder.DropColumn(
                name: "CanonicalUnit",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "ThresholdSnapshot",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "TriggeringEntityType",
                table: "UserBadge");

            migrationBuilder.DropColumn(
                name: "ProfilePictureMediaId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "PersonalRecordHistory");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "PersonalRecord");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "CanonicalUnit",
                table: "Badge");

            migrationBuilder.DropColumn(
                name: "ShowProgressWhenLocked",
                table: "Badge");

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "UserBadge",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "LegacyProfilePictureUrl",
                table: "User",
                newName: "ProfilePictureUrl");

            migrationBuilder.RenameColumn(
                name: "LegacyImageUrl",
                table: "Post",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<double>(
                name: "DurationMinutes",
                table: "GymWorkoutExercise",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reps",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sets",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "GymWorkoutExercise",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Badge",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Badge",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Badge",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Friendship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BecameFriendsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendship", x => x.Id);
                    table.CheckConstraint("CK_Friendship_NoSelf", "\"User1Id\" <> \"User2Id\"");
                    table.ForeignKey(
                        name: "FK_Friendship_User_User1Id",
                        column: x => x.User1Id,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friendship_User_User2Id",
                        column: x => x.User2Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FriendshipRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendshipRequest", x => x.Id);
                    table.CheckConstraint("CK_FriendshipRequest_Status", "\"Status\" IN ('pending', 'accepted', 'rejected')");
                    table.ForeignKey(
                        name: "FK_FriendshipRequest_User_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendshipRequest_User_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGoal_UserId",
                table: "UserGoal",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecord_UserId_WorkoutType_Metric",
                table: "PersonalRecord",
                columns: new[] { "UserId", "WorkoutType", "Metric" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_User1Id_User2Id",
                table: "Friendship",
                columns: new[] { "User1Id", "User2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_User2Id",
                table: "Friendship",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipRequest_AddresseeId",
                table: "FriendshipRequest",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipRequest_RequesterId_AddresseeId",
                table: "FriendshipRequest",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBadge_Badge_BadgeId",
                table: "UserBadge",
                column: "BadgeId",
                principalTable: "Badge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
