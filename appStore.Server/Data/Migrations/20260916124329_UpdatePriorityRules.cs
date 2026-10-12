using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriorityRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Priorities_Name",
                table: "Priorities",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Priorities_Name",
                table: "Priorities");
        }
    }
}
