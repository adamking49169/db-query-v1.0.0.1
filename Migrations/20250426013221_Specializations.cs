using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_query_v1._0._0._1.Migrations
{
    /// <inheritdoc />
    public partial class Specializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Specialization",
                table: "AspNetUsers",
                newName: "Specializations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Specializations",
                table: "AspNetUsers",
                newName: "Specialization");
        }
    }
}
