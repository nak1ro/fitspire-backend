using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.ForeignKey(
                        name: "FK_BodyCheckIn_MediaAsset_PhotoMediaId",
                        column: x => x.PhotoMediaId,
                        principalTable: "MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BodyCheckIn_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyCheckIn");
        }
    }
}
