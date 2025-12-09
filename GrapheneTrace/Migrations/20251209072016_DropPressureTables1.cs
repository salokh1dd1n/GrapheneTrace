using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrapheneTrace.Migrations
{
    /// <inheritdoc />
    public partial class DropPressureTables1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_PressureFrame_FrameId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_FrameMetrics_PressureFrame_FrameId",
                table: "FrameMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_PressureFrame_Patients_PatientId",
                table: "PressureFrame");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComments_PressureFrame_FrameId",
                table: "UserComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PressureFrame",
                table: "PressureFrame");

            migrationBuilder.RenameTable(
                name: "PressureFrame",
                newName: "PressureFrames");

            migrationBuilder.RenameIndex(
                name: "IX_PressureFrame_PatientId",
                table: "PressureFrames",
                newName: "IX_PressureFrames_PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PressureFrames",
                table: "PressureFrames",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_PressureFrames_FrameId",
                table: "Alerts",
                column: "FrameId",
                principalTable: "PressureFrames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FrameMetrics_PressureFrames_FrameId",
                table: "FrameMetrics",
                column: "FrameId",
                principalTable: "PressureFrames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PressureFrames_Patients_PatientId",
                table: "PressureFrames",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserComments_PressureFrames_FrameId",
                table: "UserComments",
                column: "FrameId",
                principalTable: "PressureFrames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_PressureFrames_FrameId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_FrameMetrics_PressureFrames_FrameId",
                table: "FrameMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_PressureFrames_Patients_PatientId",
                table: "PressureFrames");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComments_PressureFrames_FrameId",
                table: "UserComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PressureFrames",
                table: "PressureFrames");

            migrationBuilder.RenameTable(
                name: "PressureFrames",
                newName: "PressureFrame");

            migrationBuilder.RenameIndex(
                name: "IX_PressureFrames_PatientId",
                table: "PressureFrame",
                newName: "IX_PressureFrame_PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PressureFrame",
                table: "PressureFrame",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_PressureFrame_FrameId",
                table: "Alerts",
                column: "FrameId",
                principalTable: "PressureFrame",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FrameMetrics_PressureFrame_FrameId",
                table: "FrameMetrics",
                column: "FrameId",
                principalTable: "PressureFrame",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PressureFrame_Patients_PatientId",
                table: "PressureFrame",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserComments_PressureFrame_FrameId",
                table: "UserComments",
                column: "FrameId",
                principalTable: "PressureFrame",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
