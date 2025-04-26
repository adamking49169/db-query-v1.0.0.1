using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_query_v1._0._0._1.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdentityForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add both columns as nullable
            migrationBuilder.AddColumn<string>(
                name: "UserIdentityId",
                table: "ChatHistoryItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIdentityId",
                table: "PreviousChats",
                type: "nvarchar(450)",
                nullable: true);

            // 2) Back-fill with a real user ID
            migrationBuilder.Sql(
                "UPDATE ChatHistoryItems SET UserIdentityId = '00000000-0000-0000-0000-000000000000' WHERE UserIdentityId IS NULL;");
            migrationBuilder.Sql(
                "UPDATE PreviousChats SET UserIdentityId = '00000000-0000-0000-0000-000000000000' WHERE UserIdentityId IS NULL;");

            // 3) Make them NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "UserIdentityId",
                table: "ChatHistoryItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserIdentityId",
                table: "PreviousChats",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            // 4) Create the indexes
            migrationBuilder.CreateIndex(
                name: "IX_ChatHistoryItems_UserIdentityId",
                table: "ChatHistoryItems",
                column: "UserIdentityId");
            migrationBuilder.CreateIndex(
                name: "IX_PreviousChats_UserIdentityId",
                table: "PreviousChats",
                column: "UserIdentityId");

            // 5) Add the FK constraints
            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistoryItems_AspNetUsers_UserIdentityId",
                table: "ChatHistoryItems",
                column: "UserIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PreviousChats_AspNetUsers_UserIdentityId",
                table: "PreviousChats",
                column: "UserIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

    }
}
