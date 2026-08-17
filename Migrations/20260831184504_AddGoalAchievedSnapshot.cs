using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalAchievedSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SharedGoalCompletedAt",
                table: "Post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SharedGoalTargetValue",
                table: "Post",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedGoalTypeName",
                table: "Post",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedGoalUnit",
                table: "Post",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceGoalId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_SourceGoalId",
                table: "Post",
                column: "SourceGoalId",
                unique: true,
                filter: "\"SourceGoalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Post_SourceGoalId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedGoalCompletedAt",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedGoalTargetValue",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedGoalTypeName",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SharedGoalUnit",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "SourceGoalId",
                table: "Post");
        }
    }
}
