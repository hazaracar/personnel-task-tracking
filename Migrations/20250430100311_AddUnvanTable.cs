using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddUnvanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnvanId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Unvanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unvanlar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UnvanId",
                table: "AspNetUsers",
                column: "UnvanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Unvanlar_UnvanId",
                table: "AspNetUsers",
                column: "UnvanId",
                principalTable: "Unvanlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Unvanlar_UnvanId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Unvanlar");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UnvanId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnvanId",
                table: "AspNetUsers");
        }
    }
}
