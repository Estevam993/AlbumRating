using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbumRating.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlbumId = table.Column<string>(type: "TEXT", nullable: false),
                    Rate = table.Column<int>(type: "INTEGER", nullable: false),
                    Review = table.Column<string>(type: "BLOB", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumReview_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumReview_UserId",
                table: "AlbumReview",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumReview");
        }
    }
}
