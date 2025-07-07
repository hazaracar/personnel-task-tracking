using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class KurumAlaniEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                table: "GorevTalepleri");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciId",
                table: "GorevTalepleri",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Kurum",
                table: "GorevTalepleri",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                table: "GorevTalepleri",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "Kurum",
                table: "GorevTalepleri");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciId",
                table: "GorevTalepleri",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                table: "GorevTalepleri",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
