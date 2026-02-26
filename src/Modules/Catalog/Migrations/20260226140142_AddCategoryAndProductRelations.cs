using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndProductRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Catalog_Products",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Catalog_Products",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Catalog_Products",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Catalog_Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "Catalog_Products",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Catalog_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Catalog_Products",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Catalog_Products",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Catalog_Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Products_CategoryId",
                table: "Catalog_Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Products_Catalog_Categories_CategoryId",
                table: "Catalog_Products",
                column: "CategoryId",
                principalTable: "Catalog_Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Products_Catalog_Categories_CategoryId",
                table: "Catalog_Products");

            migrationBuilder.DropTable(
                name: "Catalog_Categories");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_Products_CategoryId",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Catalog_Products");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Catalog_Products");
        }
    }
}
