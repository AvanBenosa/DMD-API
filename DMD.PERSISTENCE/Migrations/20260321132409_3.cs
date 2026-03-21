using System;
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
            migrationBuilder.AddColumn<string>(
                name: "Assessment",
                table: "PatientProgressNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClinicalFinding",
                table: "PatientProgressNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextVisit",
                table: "PatientProgressNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToothNumber",
                table: "PatientProgressNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assessment",
                table: "PatientProgressNotes");

            migrationBuilder.DropColumn(
                name: "ClinicalFinding",
                table: "PatientProgressNotes");

            migrationBuilder.DropColumn(
                name: "NextVisit",
                table: "PatientProgressNotes");

            migrationBuilder.DropColumn(
                name: "ToothNumber",
                table: "PatientProgressNotes");
        }
    }
}
