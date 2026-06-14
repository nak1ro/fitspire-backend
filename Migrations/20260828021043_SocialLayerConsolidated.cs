using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SocialLayerConsolidated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FollowRequest_RequesterId_AddresseeId",
                table: "FollowRequest");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FollowRequest_Status",
                table: "FollowRequest");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");

            migrationBuilder.AddColumn<int>(
                name: "SharedCaloriesBurned",
                table: "Post",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedCompletedAt",
                table: "Post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SharedDistanceKm",
                table: "Post",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SharedDurationMinutes",
                table: "Post",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SharedExerciseCount",
                table: "Post",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SharedTotalVolumeKg",
                table: "Post",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedWorkoutDate",
                table: "Post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedWorkoutType",
                table: "Post",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceWorkoutId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FollowRequest",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FollowRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FollowRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToCommentId",
                table: "Comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RootCommentId",
                table: "Comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "FollowRequest"
                SET "Status" = CASE LOWER("Status")
                    WHEN 'pending' THEN 'Pending'
                    WHEN 'accepted' THEN 'Accepted'
                    WHEN 'rejected' THEN 'Rejected'
                    ELSE "Status"
                END,
                "CreatedAt" = "RequestedAt";
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "FollowRequest",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

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
                    table.ForeignKey(
                        name: "FK_CommentLike_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.Sql("""
                INSERT INTO "PostLike" ("Id", "UserId", "PostId", "CreatedAt", "UpdatedAt")
                SELECT "Id", "UserId", "TargetId", "CreatedAt", "UpdatedAt"
                FROM "Like"
                WHERE LOWER("TargetType") = 'post'
                  AND EXISTS (SELECT 1 FROM "Post" WHERE "Post"."Id" = "Like"."TargetId")
                ON CONFLICT ("UserId", "PostId") DO NOTHING;
                """);

            migrationBuilder.DropTable(
                name: "Like");

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourceWorkoutId",
                table: "Post",
                column: "SourceWorkoutId",
                unique: true,
                filter: "\"SourceWorkoutId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequest_RequesterId_AddresseeId",
                table: "FollowRequest",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true,
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FollowRequest_Status",
                table: "FollowRequest",
                sql: "\"Status\" IN ('Pending', 'Accepted', 'Rejected', 'Cancelled')");

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
                name: "IX_CommentLike_CommentId",
                table: "CommentLike",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLike_UserId_CommentId",
                table: "CommentLike",
                columns: new[] { "UserId", "CommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_PostId",
                table: "PostLike",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_UserId_PostId",
                table: "PostLike",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Comment_ReplyToCommentId",
                table: "Comment",
                column: "ReplyToCommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Comment_RootCommentId",
                table: "Comment",
                column: "RootCommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Comment_ReplyToCommentId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Comment_RootCommentId",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "CommentLike");

            migrationBuilder.DropIndex(
                name: "IX_Post_SourceWorkoutId",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequest_RequesterId_AddresseeId",
                table: "FollowRequest");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FollowRequest_Status",
                table: "FollowRequest");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId_RootCommentId_CreatedAt",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ReplyToCommentId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_RootCommentId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "SharedCaloriesBurned",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedCompletedAt",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedDistanceKm",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedDurationMinutes",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedExerciseCount",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedTotalVolumeKg",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedWorkoutDate",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedWorkoutType",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SourceWorkoutId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FollowRequest");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FollowRequest");

            migrationBuilder.DropColumn(
                name: "ReplyToCommentId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "RootCommentId",
                table: "Comment");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FollowRequest",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateTable(
                name: "Like",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Like", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Like_Post_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Like_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql("""
                INSERT INTO "Like" ("Id", "UserId", "TargetId", "TargetType", "CreatedAt", "UpdatedAt")
                SELECT "Id", "UserId", "PostId", 'Post', "CreatedAt", "UpdatedAt"
                FROM "PostLike"
                ON CONFLICT ("UserId", "TargetId", "TargetType") DO NOTHING;
                """);

            migrationBuilder.DropTable(
                name: "PostLike");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequest_RequesterId_AddresseeId",
                table: "FollowRequest",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FollowRequest_Status",
                table: "FollowRequest",
                sql: "\"Status\" IN ('pending', 'accepted', 'rejected')");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Like_TargetId",
                table: "Like",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Like_UserId_TargetId_TargetType",
                table: "Like",
                columns: new[] { "UserId", "TargetId", "TargetType" },
                unique: true);
        }
    }
}
