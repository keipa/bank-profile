using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRatingSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserRatingSubmissionId",
                table: "BankRatings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserRatingSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    SubmitterIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ServiceRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    FeesRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    ConvenienceRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    DigitalServicesRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    CustomerSupportRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatingSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_UserRatingSubmissions_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankRatings_UserRatingSubmissionId",
                table: "BankRatings",
                column: "UserRatingSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatingSubmissions_BankId",
                table: "UserRatingSubmissions",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatingSubmissions_SubmittedDate",
                table: "UserRatingSubmissions",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatingSubmissions_SubmitterIP_SubmittedDate",
                table: "UserRatingSubmissions",
                columns: new[] { "SubmitterIP", "SubmittedDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_BankRatings_UserRatingSubmissions_UserRatingSubmissionId",
                table: "BankRatings",
                column: "UserRatingSubmissionId",
                principalTable: "UserRatingSubmissions",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankRatings_UserRatingSubmissions_UserRatingSubmissionId",
                table: "BankRatings");

            migrationBuilder.DropTable(
                name: "UserRatingSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_BankRatings_UserRatingSubmissionId",
                table: "BankRatings");

            migrationBuilder.DropColumn(
                name: "UserRatingSubmissionId",
                table: "BankRatings");
        }
    }
}
