using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleMvcApp.Migrations
{
    public partial class FixGenre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Genre_Product_ProductId",
                table: "Genre");

            migrationBuilder.DropIndex(
                name: "IX_Genre_ProductId",
                table: "Genre");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Genre");

            migrationBuilder.CreateTable(
                name: "GenreProduct",
                columns: table => new
                {
                    GenresGenreId = table.Column<int>(type: "int", nullable: false),
                    ProductsProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreProduct", x => new { x.GenresGenreId, x.ProductsProductId });
                    table.ForeignKey(
                        name: "FK_GenreProduct_Genre_GenresGenreId",
                        column: x => x.GenresGenreId,
                        principalTable: "Genre",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreProduct_Product_ProductsProductId",
                        column: x => x.ProductsProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenreProduct_ProductsProductId",
                table: "GenreProduct",
                column: "ProductsProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenreProduct");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Genre",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genre_ProductId",
                table: "Genre",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Genre_Product_ProductId",
                table: "Genre",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
