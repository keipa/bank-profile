using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackModerationApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReviewNotes",
                table: "MetricFeedbacks",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AppliedEventId",
                table: "MetricFeedbacks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "MetricFeedbacks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetricPath",
                table: "MetricFeedbacks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "MetricFeedbacks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "MetricFeedbacks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MetricFeedbacks",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.Sql(
                """
                UPDATE mf
                SET mf.BankCode = b.BankCode
                FROM MetricFeedbacks AS mf
                INNER JOIN Banks AS b ON b.BankId = mf.BankId
                WHERE mf.BankCode IS NULL
                  AND mf.BankId IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_BankCode",
                table: "MetricFeedbacks",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_MetricPath",
                table: "MetricFeedbacks",
                column: "MetricPath");

            migrationBuilder.CreateIndex(
                name: "IX_MetricFeedbacks_ReviewedDate",
                table: "MetricFeedbacks",
                column: "ReviewedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetricFeedbacks_BankCode",
                table: "MetricFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_MetricFeedbacks_MetricPath",
                table: "MetricFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_MetricFeedbacks_ReviewedDate",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "AppliedEventId",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "MetricPath",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "MetricFeedbacks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MetricFeedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewNotes",
                table: "MetricFeedbacks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
