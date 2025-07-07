using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddKullaniciToGorev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KullaniciId",
                table: "Gorevs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Gorevs_KullaniciId",
                table: "Gorevs",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_LoginInfos_KullaniciId",
                table: "Gorevs",
                column: "KullaniciId",
                principalTable: "LoginInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_LoginInfos_KullaniciId",
                table: "Gorevs");

            migrationBuilder.DropIndex(
                name: "IX_Gorevs_KullaniciId",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Gorevs");
        }
    }
}
