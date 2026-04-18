using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBankOnboardingSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankOnboardingSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposedBankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProposedCountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProposedWebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SubmissionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    SubmitterIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ApprovedBankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankOnboardingSubmissions", x => x.SubmissionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankOnboardingSubmissions_ApprovedBankCode",
                table: "BankOnboardingSubmissions",
                column: "ApprovedBankCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankOnboardingSubmissions_Status",
                table: "BankOnboardingSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BankOnboardingSubmissions_SubmittedDate",
                table: "BankOnboardingSubmissions",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankOnboardingSubmissions_SubmitterIP_SubmittedDate",
                table: "BankOnboardingSubmissions",
                columns: new[] { "SubmitterIP", "SubmittedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankOnboardingSubmissions");
        }
    }
}
