using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FribergCarRentalsAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeededUsersAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "b224aef4-90cc-4178-951d-60c5863cef30", null, "User", "User" },
                    { "e27d61dc-c6ff-4f09-9777-d03cdba1c28f", null, "Administrator", "Administrator" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "111786f7-e6d4-40bc-94d2-bf42fed88f3d", 0, "804c403d-949c-4b50-a650-20d230689ffd", "admin@demoapi.com", true, "System", "Admin", false, null, "ADMIN@DEMOAPI.COM", "ADMIN@DEMOAPI.COM", "AQAAAAIAAYagAAAAELAVWEcptR2Z1GevgYjJq13BKzWjVhY2Kzpf9PToTDMHf2fFFNlnCXGbrDFvgeoe6w==", null, false, "4c6e020a-390d-491b-a283-dc8c827345a8", false, "admin@demoapi.com" },
                    { "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e", 0, "118e6983-9de0-4501-80f2-cc6e0c3481bf", "user@demoapi.com", true, "System", "User", false, null, "USER@DEMOAPI.COM", "USER@DEMOAPI.COM", "AQAAAAIAAYagAAAAEGIEgrETiBX+qqYrrTtfWhazQGxZUcWhbNhgsRhxy0Lqcqo9WRyABATkakMVgXjxMg==", null, false, "97ff03d6-50c7-4b01-8b6f-6e7481e5b67b", false, "user@demoapi.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "e27d61dc-c6ff-4f09-9777-d03cdba1c28f", "111786f7-e6d4-40bc-94d2-bf42fed88f3d" },
                    { "b224aef4-90cc-4178-951d-60c5863cef30", "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "e27d61dc-c6ff-4f09-9777-d03cdba1c28f", "111786f7-e6d4-40bc-94d2-bf42fed88f3d" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "b224aef4-90cc-4178-951d-60c5863cef30", "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b224aef4-90cc-4178-951d-60c5863cef30");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e27d61dc-c6ff-4f09-9777-d03cdba1c28f");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "111786f7-e6d4-40bc-94d2-bf42fed88f3d");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e");
        }
    }
}
