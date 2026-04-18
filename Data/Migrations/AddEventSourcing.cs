using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSourcing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankSnapshots",
                columns: table => new
                {
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfileJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventSequenceUpTo = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankSnapshots", x => x.SnapshotId);
                });

            migrationBuilder.CreateTable(
                name: "MetricEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetricValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventVersion = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    EventSequence = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricEvents", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankSnapshots_BankCode_EventSequenceUpTo",
                table: "BankSnapshots",
                columns: new[] { "BankCode", "EventSequenceUpTo" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricEvents_BankCode",
                table: "MetricEvents",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_MetricEvents_BankCode_EventSequence",
                table: "MetricEvents",
                columns: new[] { "BankCode", "EventSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetricEvents_BankCode_MetricName_CreatedDate",
                table: "MetricEvents",
                columns: new[] { "BankCode", "MetricName", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankSnapshots");

            migrationBuilder.DropTable(
                name: "MetricEvents");
        }
    }
}
