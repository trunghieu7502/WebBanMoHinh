using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_WebBanMoHinh.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFavProducts4th : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_AspNetUsers_UserId",
                table: "FavoriteProducts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FavoriteProducts",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteProducts_UserId",
                table: "FavoriteProducts",
                newName: "IX_FavoriteProducts_UserID");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "FavoriteProducts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_AspNetUsers_UserID",
                table: "FavoriteProducts",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_AspNetUsers_UserID",
                table: "FavoriteProducts");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "FavoriteProducts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteProducts_UserID",
                table: "FavoriteProducts",
                newName: "IX_FavoriteProducts_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FavoriteProducts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_AspNetUsers_UserId",
                table: "FavoriteProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
