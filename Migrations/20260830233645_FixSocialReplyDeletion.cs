using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixSocialReplyDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Comment_ReplyToCommentId",
                table: "Comment");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Comment_ReplyToCommentId",
                table: "Comment",
                column: "ReplyToCommentId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Comment_ReplyToCommentId",
                table: "Comment",
                column: "ReplyToCommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
