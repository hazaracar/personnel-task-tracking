using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyKurumToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Kurum",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "KurumId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers",
                column: "KurumId",
                principalTable: "Kurumlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "KurumId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Kurum",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Kurumlar_KurumId",
                table: "AspNetUsers",
                column: "KurumId",
                principalTable: "Kurumlar",
                principalColumn: "Id");
        }
    }
}
