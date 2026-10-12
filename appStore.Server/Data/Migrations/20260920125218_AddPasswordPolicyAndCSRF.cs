using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordPolicyAndCSRF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MinimumLength = table.Column<int>(type: "INTEGER", nullable: false),
                    RequireUppercase = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireLowercase = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireDigit = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireSpecialCharacter = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordPolicies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordPolicies");
        }
    }
}
