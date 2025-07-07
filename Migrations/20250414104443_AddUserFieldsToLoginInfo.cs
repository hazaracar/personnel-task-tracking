using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFieldsToLoginInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Birim",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CalismaSehri",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cinsiyet",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kurum",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SicilNo",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TcKimlikNo",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unvan",
                table: "LoginInfos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birim",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "CalismaSehri",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "Cinsiyet",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "Kurum",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "SicilNo",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "TcKimlikNo",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "Unvan",
                table: "LoginInfos");
        }
    }
}
