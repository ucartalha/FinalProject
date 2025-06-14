using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig15062025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnals_Departments_DepartmentId",
                table: "Personnals");

            migrationBuilder.DropTable(
                name: "RemoteWorkEmployees");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Personnals");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Personnals",
                newName: "UserName");

            migrationBuilder.AddColumn<int>(
                name: "PersonnalId",
                table: "ReaderDataDtos",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Personnals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PersonnalId",
                table: "EmployeeRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinalVpnEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemoteEmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstRecord = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRecord = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    BytesOut = table.Column<long>(type: "bigint", nullable: true),
                    BytesIn = table.Column<long>(type: "bigint", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalVpnEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VpnEmployees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BytesOut = table.Column<long>(type: "bigint", nullable: true),
                    BytesIn = table.Column<long>(type: "bigint", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    RemoteEmployeeId = table.Column<int>(type: "int", nullable: false),
                    FirstRecord = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRecord = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnEmployees", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReaderDataDtos_PersonnalId",
                table: "ReaderDataDtos",
                column: "PersonnalId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRecords_PersonnalId",
                table: "EmployeeRecords",
                column: "PersonnalId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecords_Personnals_PersonnalId",
                table: "EmployeeRecords",
                column: "PersonnalId",
                principalTable: "Personnals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Personnals_Departments_DepartmentId",
                table: "Personnals",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReaderDataDtos_Personnals_PersonnalId",
                table: "ReaderDataDtos",
                column: "PersonnalId",
                principalTable: "Personnals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecords_Personnals_PersonnalId",
                table: "EmployeeRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Personnals_Departments_DepartmentId",
                table: "Personnals");

            migrationBuilder.DropForeignKey(
                name: "FK_ReaderDataDtos_Personnals_PersonnalId",
                table: "ReaderDataDtos");

            migrationBuilder.DropTable(
                name: "FinalVpnEmployees");

            migrationBuilder.DropTable(
                name: "VpnEmployees");

            migrationBuilder.DropIndex(
                name: "IX_ReaderDataDtos_PersonnalId",
                table: "ReaderDataDtos");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRecords_PersonnalId",
                table: "EmployeeRecords");

            migrationBuilder.DropColumn(
                name: "PersonnalId",
                table: "ReaderDataDtos");

            migrationBuilder.DropColumn(
                name: "PersonnalId",
                table: "EmployeeRecords");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Personnals",
                newName: "PhoneNumber");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Personnals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Personnals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "RemoteWorkEmployees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemoteDuration = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteWorkEmployees", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Personnals_Departments_DepartmentId",
                table: "Personnals",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
