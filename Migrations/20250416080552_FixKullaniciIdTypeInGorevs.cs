using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class FixKullaniciIdTypeInGorevs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_LoginInfos_KullaniciId",
                table: "Gorevs");

            migrationBuilder.DropTable(
                name: "LoginInfos");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciId",
                table: "Gorevs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_AspNetUsers_KullaniciId",
                table: "Gorevs",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevs_AspNetUsers_KullaniciId",
                table: "Gorevs");

            migrationBuilder.AlterColumn<int>(
                name: "KullaniciId",
                table: "Gorevs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "LoginInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Birim = table.Column<string>(type: "text", nullable: false),
                    CalismaSehri = table.Column<string>(type: "text", nullable: false),
                    Cinsiyet = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    Kurum = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    SicilNo = table.Column<string>(type: "text", nullable: false),
                    TcKimlikNo = table.Column<string>(type: "text", nullable: false),
                    Unvan = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginInfos", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevs_LoginInfos_KullaniciId",
                table: "Gorevs",
                column: "KullaniciId",
                principalTable: "LoginInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
