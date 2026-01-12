using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSignGestures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SignGestures",
                columns: new[] { "Id", "DescriptionAR", "DescriptionEN", "ImageUrl", "Token", "TokenType" },
                values: new object[,]
                {
                    { 101, "إشارة الحرف A", "Sign for letter A", "signs/A.svg", "A", "Letter" },
                    { 102, "إشارة الحرف B", "Sign for letter B", "signs/B.svg", "B", "Letter" },
                    { 103, "إشارة الحرف C", "Sign for letter C", "signs/C.svg", "C", "Letter" },
                    { 104, "إشارة الحرف D", "Sign for letter D", "signs/D.svg", "D", "Letter" },
                    { 105, "إشارة الحرف E", "Sign for letter E", "signs/E.svg", "E", "Letter" },
                    { 106, "إشارة الحرف F", "Sign for letter F", "signs/F.svg", "F", "Letter" },
                    { 107, "إشارة الحرف G", "Sign for letter G", "signs/G.svg", "G", "Letter" },
                    { 108, "إشارة الحرف H", "Sign for letter H", "signs/H.svg", "H", "Letter" },
                    { 109, "إشارة الحرف I", "Sign for letter I", "signs/I.svg", "I", "Letter" },
                    { 110, "إشارة الحرف J", "Sign for letter J", "signs/J.svg", "J", "Letter" },
                    { 111, "إشارة الحرف K", "Sign for letter K", "signs/K.svg", "K", "Letter" },
                    { 112, "إشارة الحرف L", "Sign for letter L", "signs/L.svg", "L", "Letter" },
                    { 113, "إشارة الحرف M", "Sign for letter M", "signs/M.svg", "M", "Letter" },
                    { 114, "إشارة الحرف N", "Sign for letter N", "signs/N.svg", "N", "Letter" },
                    { 115, "إشارة الحرف O", "Sign for letter O", "signs/O.svg", "O", "Letter" },
                    { 116, "إشارة الحرف P", "Sign for letter P", "signs/P.svg", "P", "Letter" },
                    { 117, "إشارة الحرف Q", "Sign for letter Q", "signs/Q.svg", "Q", "Letter" },
                    { 118, "إشارة الحرف R", "Sign for letter R", "signs/R.svg", "R", "Letter" },
                    { 119, "إشارة الحرف S", "Sign for letter S", "signs/S.svg", "S", "Letter" },
                    { 120, "إشارة الحرف T", "Sign for letter T", "signs/T.svg", "T", "Letter" },
                    { 121, "إشارة الحرف U", "Sign for letter U", "signs/U.svg", "U", "Letter" },
                    { 122, "إشارة الحرف V", "Sign for letter V", "signs/V.svg", "V", "Letter" },
                    { 123, "إشارة الحرف W", "Sign for letter W", "signs/W.svg", "W", "Letter" },
                    { 124, "إشارة الحرف X", "Sign for letter X", "signs/X.svg", "X", "Letter" },
                    { 125, "إشارة الحرف Y", "Sign for letter Y", "signs/Y.svg", "Y", "Letter" },
                    { 126, "إشارة الحرف Z", "Sign for letter Z", "signs/Z.svg", "Z", "Letter" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignGestures_TokenType_Token",
                table: "SignGestures",
                columns: new[] { "TokenType", "Token" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SignGestures_TokenType_Token",
                table: "SignGestures");

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "SignGestures",
                keyColumn: "Id",
                keyValue: 126);
        }
    }
}
