using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProfiles.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixViewHistoryViewCountDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ViewCount",
                table: "ViewHistory",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ViewCount",
                table: "ViewHistory",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
