using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class SiliconPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "silicon_voice",
                table: "profile_role_loadout",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "station_ai_hologram",
                table: "profile_role_loadout",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "station_ai_screen",
                table: "profile_role_loadout",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "silicon_voice",
                table: "profile_role_loadout");

            migrationBuilder.DropColumn(
                name: "station_ai_hologram",
                table: "profile_role_loadout");

            migrationBuilder.DropColumn(
                name: "station_ai_screen",
                table: "profile_role_loadout");
        }
    }
}
