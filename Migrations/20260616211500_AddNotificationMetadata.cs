using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notification",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "ActorUserId",
                table: "Notification",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceEntityId",
                table: "Notification",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceEntityType",
                table: "Notification",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notification",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notification",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Follow");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notification",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "Notification",
                columns: new[] { "UserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ReferenceEntityId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ReferenceEntityType",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");
        }
    }
}
