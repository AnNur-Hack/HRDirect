using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRDirectAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportingManagerId = table.Column<int>(type: "int", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginWindowStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ReportingManagerId",
                        column: x => x.ReportingManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EstacodeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DailyRateUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstacodeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvalidatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTokens_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrainingEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CourseFeeCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedFlightCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FlightCostCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AssignedManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerDecisionAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRDecisionAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinanceDecisionAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinanceComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WithdrawnAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionEvidencePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionEvidenceFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionEvidenceContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionEvidenceComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionEvidenceSubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionEvidenceSubmittedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    CompletionDecisionComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRequests_Employees_AssignedManagerId",
                        column: x => x.AssignedManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRoleAssignments",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRoleAssignments", x => new { x.EmployeeId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_EmployeeRoleAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAllowanceVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingRequestId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyRateUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EligibleDays = table.Column<int>(type: "int", nullable: false),
                    TotalAllowance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorrectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAllowanceVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingAllowanceVersions_TrainingRequests_TrainingRequestId",
                        column: x => x.TrainingRequestId,
                        principalTable: "TrainingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Staff" },
                    { 2, "Manager" },
                    { 3, "HR" },
                    { 4, "Finance" },
                    { 5, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountTokens_EmployeeId",
                table: "AccountTokens",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmployeeId",
                table: "AuditLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoleAssignments_RoleId",
                table: "EmployeeRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ReportingManagerId",
                table: "Employees",
                column: "ReportingManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_EstacodeRates_CountryCode_Grade",
                table: "EstacodeRates",
                columns: new[] { "CountryCode", "Grade" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAllowanceVersions_TrainingRequestId_VersionNumber",
                table: "TrainingAllowanceVersions",
                columns: new[] { "TrainingRequestId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequests_AssignedManagerId",
                table: "TrainingRequests",
                column: "AssignedManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequests_EmployeeId",
                table: "TrainingRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeeRoleAssignments");

            migrationBuilder.DropTable(
                name: "EstacodeRates");

            migrationBuilder.DropTable(
                name: "TrainingAllowanceVersions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TrainingRequests");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
