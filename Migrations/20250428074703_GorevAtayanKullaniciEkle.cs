using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class GorevAtayanKullaniciEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AtayanKullaniciId",
                table: "Gorevs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevs_AtayanKullaniciId",
                table: "Gorevs",
                column: "AtayanKullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_AspNetUsers_AtayanKullaniciId",
                table: "Gorevs",
                column: "AtayanKullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_AspNetUsers_AtayanKullaniciId",
                table: "Gorevs");

            migrationBuilder.DropIndex(
                name: "IX_Gorevs_AtayanKullaniciId",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "AtayanKullaniciId",
                table: "Gorevs");
        }
    }
}
