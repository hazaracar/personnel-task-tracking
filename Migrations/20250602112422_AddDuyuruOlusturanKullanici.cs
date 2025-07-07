using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddDuyuruOlusturanKullanici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OlusturanKullaniciId",
                table: "Duyurular",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Duyurular_OlusturanKullaniciId",
                table: "Duyurular",
                column: "OlusturanKullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Duyurular_AspNetUsers_OlusturanKullaniciId",
                table: "Duyurular",
                column: "OlusturanKullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duyurular_AspNetUsers_OlusturanKullaniciId",
                table: "Duyurular");

            migrationBuilder.DropIndex(
                name: "IX_Duyurular_OlusturanKullaniciId",
                table: "Duyurular");

            migrationBuilder.DropColumn(
                name: "OlusturanKullaniciId",
                table: "Duyurular");
        }
    }
}
