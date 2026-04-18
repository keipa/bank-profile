using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmitterIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSubmissions", x => x.SubmissionId);
                });

            migrationBuilder.CreateTable(
                name: "MetricFeedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    MetricCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuggestedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SubmitterIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricFeedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_MetricFeedbacks_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSubmissions_SubmissionDate",
                table: "FeedbackSubmissions",
                column: "SubmissionDate");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSubmissions_SubmitterIP",
                table: "FeedbackSubmissions",
                column: "SubmitterIP");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSubmissions_SubmitterIP_SubmissionDate",
                table: "FeedbackSubmissions",
                columns: new[] { "SubmitterIP", "SubmissionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_BankId",
                table: "MetricFeedbacks",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_Status",
                table: "MetricFeedbacks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_SubmittedDate",
                table: "MetricFeedbacks",
                column: "SubmittedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackSubmissions");

            migrationBuilder.DropTable(
                name: "MetricFeedbacks");
        }
    }
}
