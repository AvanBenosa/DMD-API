using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD.PERSISTENCE.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplate_ClinicProfiles_ClinicProfileId",
                table: "FormTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormTemplate",
                table: "FormTemplate");

            migrationBuilder.RenameTable(
                name: "FormTemplate",
                newName: "FormTemplates");

            migrationBuilder.RenameIndex(
                name: "IX_FormTemplate_ClinicProfileId",
                table: "FormTemplates",
                newName: "IX_FormTemplates_ClinicProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormTemplates",
                table: "FormTemplates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplates_ClinicProfiles_ClinicProfileId",
                table: "FormTemplates",
                column: "ClinicProfileId",
                principalTable: "ClinicProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplates_ClinicProfiles_ClinicProfileId",
                table: "FormTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormTemplates",
                table: "FormTemplates");

            migrationBuilder.RenameTable(
                name: "FormTemplates",
                newName: "FormTemplate");

            migrationBuilder.RenameIndex(
                name: "IX_FormTemplates_ClinicProfileId",
                table: "FormTemplate",
                newName: "IX_FormTemplate_ClinicProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormTemplate",
                table: "FormTemplate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplate_ClinicProfiles_ClinicProfileId",
                table: "FormTemplate",
                column: "ClinicProfileId",
                principalTable: "ClinicProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
