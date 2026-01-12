using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceText = table.Column<string>(type: "TEXT", nullable: false),
                    TargetSystem = table.Column<string>(type: "TEXT", nullable: false),
                    ResultPayload = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    TitleAR = table.Column<string>(type: "TEXT", nullable: false),
                    TitleEN = table.Column<string>(type: "TEXT", nullable: false),
                    BodyAR = table.Column<string>(type: "TEXT", nullable: false),
                    BodyEN = table.Column<string>(type: "TEXT", nullable: false),
                    MediaUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MorseMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Char = table.Column<string>(type: "TEXT", nullable: false),
                    Pattern = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MorseMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignGestures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenType = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionAR = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionEN = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignGestures", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MorseMappings",
                columns: new[] { "Id", "Char", "Pattern" },
                values: new object[,]
                {
                    { 1, "A", ".-" },
                    { 2, "B", "-..." },
                    { 3, "C", "-.-." },
                    { 4, "D", "-.." },
                    { 5, "E", "." },
                    { 6, "F", "..-." },
                    { 7, "G", "--." },
                    { 8, "H", "...." },
                    { 9, "I", ".." },
                    { 10, "J", ".---" },
                    { 11, "K", "-.-" },
                    { 12, "L", ".-.." },
                    { 13, "M", "--" },
                    { 14, "N", "-." },
                    { 15, "O", "---" },
                    { 16, "P", ".--." },
                    { 17, "Q", "--.-" },
                    { 18, "R", ".-." },
                    { 19, "S", "..." },
                    { 20, "T", "-" },
                    { 21, "U", "..-" },
                    { 22, "V", "...-" },
                    { 23, "W", ".--" },
                    { 24, "X", "-..-" },
                    { 25, "Y", "-.--" },
                    { 26, "Z", "--.." },
                    { 27, "0", "-----" },
                    { 28, "1", ".----" },
                    { 29, "2", "..---" },
                    { 30, "3", "...--" },
                    { 31, "4", "....-" },
                    { 32, "5", "....." },
                    { 33, "6", "-...." },
                    { 34, "7", "--..." },
                    { 35, "8", "---.." },
                    { 36, "9", "----." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MorseMappings_Char",
                table: "MorseMappings",
                column: "Char",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionHistories");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "MorseMappings");

            migrationBuilder.DropTable(
                name: "SignGestures");
        }
    }
}
