using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGorevModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ilce",
                table: "Gorevs",
                newName: "YoneticiAciklama");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "Gorevs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BitisTarihi",
                table: "Gorevs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "Gorevs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HarcamaTutari",
                table: "Gorevs",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IptalEdildiMi",
                table: "Gorevs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KonaklamaTuru",
                table: "Gorevs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kurum",
                table: "Gorevs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TalepId",
                table: "Gorevs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UlasimTuru",
                table: "Gorevs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "YemekTutari",
                table: "Gorevs",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Gorevs_TalepId",
                table: "Gorevs",
                column: "TalepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_GorevTalepleri_TalepId",
                table: "Gorevs",
                column: "TalepId",
                principalTable: "GorevTalepleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_GorevTalepleri_TalepId",
                table: "Gorevs");

            migrationBuilder.DropIndex(
                name: "IX_Gorevs_TalepId",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "HarcamaTutari",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "IptalEdildiMi",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "KonaklamaTuru",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "Kurum",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "TalepId",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "UlasimTuru",
                table: "Gorevs");

            migrationBuilder.DropColumn(
                name: "YemekTutari",
                table: "Gorevs");

            migrationBuilder.RenameColumn(
                name: "YoneticiAciklama",
                table: "Gorevs",
                newName: "Ilce");
        }
    }
}
