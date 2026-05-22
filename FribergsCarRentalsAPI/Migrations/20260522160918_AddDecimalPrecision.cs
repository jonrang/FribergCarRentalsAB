using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FribergCarRentalsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0b281efb-70c5-4751-814a-bb6c0baac936", null, "Suspended", "Suspended" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0b281efb-70c5-4751-814a-bb6c0baac936");
        }
    }
}
