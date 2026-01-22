using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedExerciseIdToGoalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CyclingWorkoutDetails");

            migrationBuilder.DropTable(
                name: "ProgressEntry");

            migrationBuilder.DropTable(
                name: "SwimmingWorkoutDetails");

            migrationBuilder.DropTable(
                name: "YogaWorkoutDetails");

            migrationBuilder.DropTable(
                name: "Goal");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "IsRoutine",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "RoutineName",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "GoalType");

            migrationBuilder.AddColumn<int>(
                name: "CaloriesBurned",
                table: "UserWorkout",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "UserWorkout",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserWorkout",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserWorkout",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "UserPreference",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PersonalRecord",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "GymWorkoutExercise",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Sets",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Reps",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderIndex",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GymWorkoutExercise",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GymWorkoutExercise",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GoalType",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "GoalType",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GoalType",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "GoalType",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GoalType",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DefaultUnit",
                table: "GoalType",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeasurementType",
                table: "GoalType",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedExerciseId",
                table: "GoalType",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedMetric",
                table: "GoalType",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedWorkoutType",
                table: "GoalType",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GoalType",
                type: "timestamp with time zone",
                nullable: true);

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
                    table.ForeignKey(
                        name: "FK_GoalProgressEntry_UserGoal_GoalId",
                        column: x => x.GoalId,
                        principalTable: "UserGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalProgressEntry_GoalId",
                table: "GoalProgressEntry",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoal_GoalTypeId",
                table: "UserGoal",
                column: "GoalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoal_UserId",
                table: "UserGoal",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutines_UserId",
                table: "WorkoutRoutines",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CyclingUserWorkouts");

            migrationBuilder.DropTable(
                name: "GoalProgressEntry");

            migrationBuilder.DropTable(
                name: "RunningUserWorkouts");

            migrationBuilder.DropTable(
                name: "SwimmingUserWorkouts");

            migrationBuilder.DropTable(
                name: "WorkoutRoutines");

            migrationBuilder.DropTable(
                name: "YogaUserWorkouts");

            migrationBuilder.DropTable(
                name: "UserGoal");

            migrationBuilder.DropColumn(
                name: "CaloriesBurned",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserWorkout");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "UserPreference");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PersonalRecord");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GymWorkoutExercise");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "DefaultUnit",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "MeasurementType",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "RelatedExerciseId",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "RelatedMetric",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "RelatedWorkoutType",
                table: "GoalType");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GoalType");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserWorkout",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRoutine",
                table: "UserWorkout",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoutineName",
                table: "UserWorkout",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "UserWorkout",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "GymWorkoutExercise",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "Sets",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Reps",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "OrderIndex",
                table: "GymWorkoutExercise",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GoalType",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "GoalType",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GoalType",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "GoalType",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "GoalType",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CyclingWorkoutDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvgSpeedKmPerHour = table.Column<double>(type: "double precision", nullable: true),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    ElevationGain = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyclingWorkoutDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CyclingWorkoutDetails_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentValue = table.Column<float>(type: "real", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetValue = table.Column<float>(type: "real", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goal_GoalType_GoalTypeId",
                        column: x => x.GoalTypeId,
                        principalTable: "GoalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Goal_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwimmingWorkoutDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    Laps = table.Column<int>(type: "integer", nullable: true),
                    StrokeType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwimmingWorkoutDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwimmingWorkoutDetails_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YogaWorkoutDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FocusArea = table.Column<string>(type: "text", nullable: true),
                    Intensity = table.Column<string>(type: "text", nullable: true),
                    Style = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YogaWorkoutDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YogaWorkoutDetails_UserWorkout_Id",
                        column: x => x.Id,
                        principalTable: "UserWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyFat = table.Column<float>(type: "real", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressEntry_Goal_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProgressEntry_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goal_GoalTypeId",
                table: "Goal",
                column: "GoalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Goal_UserId",
                table: "Goal",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressEntry_GoalId",
                table: "ProgressEntry",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressEntry_UserId",
                table: "ProgressEntry",
                column: "UserId");
        }
    }
}
