using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class RecreateGorevTalepleriWithEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GorevTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TalepTuru = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false),
                    KullaniciId = table.Column<string>(type: "text", nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanliMi = table.Column<bool>(type: "boolean", nullable: false),
                    TalepEdilenPersonelSayisi = table.Column<int>(type: "integer", nullable: false),
                    Kurum = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GorevTalepleri_KullaniciId",
                table: "GorevTalepleri",
                column: "KullaniciId");
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_GorevTalepleri_TalepId",
                table: "Gorevs");

            migrationBuilder.DropForeignKey(
                name: "FK_GorevTalepleri_AspNetUsers_KullaniciId",
                table: "GorevTalepleri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GorevTalepleri",
                table: "GorevTalepleri");

            migrationBuilder.DropIndex(
                name: "IX_GorevTalepleri_KullaniciId",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "Kurum",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "OlusturmaTarihi",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "PlanliMi",
                table: "GorevTalepleri");

            migrationBuilder.DropColumn(
                name: "TalepEdilenPersonelSayisi",
                table: "GorevTalepleri");

            migrationBuilder.RenameTable(
                name: "GorevTalepleri",
                newName: "GorevTalebi");

            migrationBuilder.RenameColumn(
                name: "TalepTuru",
                table: "GorevTalebi",
                newName: "TempId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GorevTalebi_TempId",
                table: "GorevTalebi",
                column: "TempId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_GorevTalebi_TalepId",
                table: "Gorevs",
                column: "TalepId",
                principalTable: "GorevTalebi",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GorevTalebi_AspNetUsers_KullaniciId",
                table: "GorevTalebi",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
