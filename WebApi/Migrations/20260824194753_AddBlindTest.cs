using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBlindTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlindTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlindTestTrack",
                columns: table => new
                {
                    BlindTestsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TracksId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindTestTrack", x => new { x.BlindTestsId, x.TracksId });
                    table.ForeignKey(
                        name: "FK_BlindTestTrack_BlindTests_BlindTestsId",
                        column: x => x.BlindTestsId,
                        principalTable: "BlindTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlindTestTrack_Tracks_TracksId",
                        column: x => x.TracksId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlindTestTrack_TracksId",
                table: "BlindTestTrack",
                column: "TracksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlindTestTrack");

            migrationBuilder.DropTable(
                name: "BlindTests");
        }
    }
}
