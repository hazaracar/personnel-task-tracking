using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddKurumModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KurumId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kurumlar",
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
                    table.PrimaryKey("PK_Kurumlar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_KurumId",
                table: "AspNetUsers",
                column: "KurumId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers",
                column: "KurumId",
                principalTable: "Kurumlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Kurumlar");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_KurumId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "KurumId",
                table: "AspNetUsers");
        }
    }
}
