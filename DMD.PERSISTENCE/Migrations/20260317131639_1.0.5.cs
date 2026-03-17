using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD.PERSISTENCE.Migrations
{
    /// <inheritdoc />
    public partial class _105 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentRemarks");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "AppointmentRequests");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "AppointmentRequests");

            migrationBuilder.RenameColumn(
                name: "PatientName",
                table: "AppointmentRequests",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "Dentist",
                table: "AppointmentRequests",
                newName: "PatientInfoId");

            migrationBuilder.RenameColumn(
                name: "AppointmentDate",
                table: "AppointmentRequests",
                newName: "AppointmentDateTo");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDateFrom",
                table: "AppointmentRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PatientInfoId1",
                table: "AppointmentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_PatientInfoId1",
                table: "AppointmentRequests",
                column: "PatientInfoId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentRequests_PatientInfos_PatientInfoId1",
                table: "AppointmentRequests",
                column: "PatientInfoId1",
                principalTable: "PatientInfos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentRequests_PatientInfos_PatientInfoId1",
                table: "AppointmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentRequests_PatientInfoId1",
                table: "AppointmentRequests");

            migrationBuilder.DropColumn(
                name: "AppointmentDateFrom",
                table: "AppointmentRequests");

            migrationBuilder.DropColumn(
                name: "PatientInfoId1",
                table: "AppointmentRequests");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "AppointmentRequests",
                newName: "PatientName");

            migrationBuilder.RenameColumn(
                name: "PatientInfoId",
                table: "AppointmentRequests",
                newName: "Dentist");

            migrationBuilder.RenameColumn(
                name: "AppointmentDateTo",
                table: "AppointmentRequests",
                newName: "AppointmentDate");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "AppointmentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "AppointmentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppointmentRemarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentRequestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRemarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentRemarks_AppointmentRequests_AppointmentRequestId",
                        column: x => x.AppointmentRequestId,
                        principalTable: "AppointmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRemarks_AppointmentRequestId",
                table: "AppointmentRemarks",
                column: "AppointmentRequestId");
        }
    }
}
