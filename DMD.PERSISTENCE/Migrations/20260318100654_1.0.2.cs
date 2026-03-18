using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD.PERSISTENCE.Migrations
{
    /// <inheritdoc />
    public partial class _102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDataPrivacyAccepted",
                table: "ClinicProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDataPrivacyAccepted",
                table: "ClinicProfiles");
        }
    }
}
