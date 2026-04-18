using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastViewedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "RatingCriterias",
                columns: table => new
                {
                    CriteriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingCriterias", x => x.CriteriaId);
                });

            migrationBuilder.CreateTable(
                name: "BankRatings",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    CriteriaId = table.Column<int>(type: "int", nullable: false),
                    RatingValue = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    RatingDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankRatings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_BankRatings_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankRatings_RatingCriterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "RatingCriterias",
                        principalColumn: "CriteriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RatingHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    CriteriaId = table.Column<int>(type: "int", nullable: false),
                    OverallRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_RatingHistories_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatingHistories_RatingCriterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "RatingCriterias",
                        principalColumn: "CriteriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RatingCriterias",
                columns: new[] { "CriteriaId", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Service" },
                    { 2, 2, "Fees" },
                    { 3, 3, "Convenience" },
                    { 4, 4, "Digital Services" },
                    { 5, 5, "Customer Support" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankRatings_BankId_CriteriaId",
                table: "BankRatings",
                columns: new[] { "BankId", "CriteriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankRatings_CriteriaId",
                table: "BankRatings",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_BankCode",
                table: "Banks",
                column: "BankCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_ViewCount",
                table: "Banks",
                column: "ViewCount");

            migrationBuilder.CreateIndex(
                name: "IX_RatingCriterias_DisplayOrder",
                table: "RatingCriterias",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_RatingHistories_BankId_CriteriaId_RecordedDate",
                table: "RatingHistories",
                columns: new[] { "BankId", "CriteriaId", "RecordedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RatingHistories_CriteriaId",
                table: "RatingHistories",
                column: "CriteriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankRatings");

            migrationBuilder.DropTable(
                name: "RatingHistories");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "RatingCriterias");
        }
    }
}
