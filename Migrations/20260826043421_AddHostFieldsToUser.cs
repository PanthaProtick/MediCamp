using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MediCamp.Migrations
{
    /// <inheritdoc />
    public partial class AddHostFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Division = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Union = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Village = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterMedicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GenericName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DosageForm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEssential = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMedicines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    NID = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    BloodGroup = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OrganizationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrganizationRegNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FocalPersonContact = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HostApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HostRejectionReason = table.Column<string>(type: "text", nullable: true),
                    MedicalSpecialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BMDCRegNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloodDonationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsAvailableDonor = table.Column<bool>(type: "boolean", nullable: false),
                    BloodGroup = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastDonatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalDonationsCount = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodDonationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodDonationProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Camps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CampType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Upazila = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Venue = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedPatients = table.Column<int>(type: "integer", nullable: false),
                    RegisteredPatientsCount = table.Column<int>(type: "integer", nullable: false),
                    ServedPatientsCount = table.Column<int>(type: "integer", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UtilizedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HostId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Camps_Users_HostId",
                        column: x => x.HostId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampId = table.Column<int>(type: "integer", nullable: false),
                    MasterMedicineId = table.Column<int>(type: "integer", nullable: false),
                    QuantityAllocated = table.Column<int>(type: "integer", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampInventories_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampInventories_MasterMedicines_MasterMedicineId",
                        column: x => x.MasterMedicineId,
                        principalTable: "MasterMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TriageRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    VolunteerId = table.Column<string>(type: "text", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BloodPressure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TemperatureF = table.Column<double>(type: "double precision", nullable: true),
                    WeightKg = table.Column<double>(type: "double precision", nullable: true),
                    HeightCm = table.Column<double>(type: "double precision", nullable: true),
                    BMI = table.Column<double>(type: "double precision", nullable: true),
                    PresentingSymptoms = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UrgencyLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TokenNumber = table.Column<int>(type: "integer", nullable: false),
                    IsSeenByDoctor = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriageRecords_Camps_CampId",
                        column: x => x.CampId,
                        principalTable: "Camps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriageRecords_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriageRecords_Users_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriageRecordId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<string>(type: "text", nullable: false),
                    ConsultedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Diagnosis = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Advice = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultations_TriageRecords_TriageRecordId",
                        column: x => x.TriageRecordId,
                        principalTable: "TriageRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultations_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsultationId = table.Column<int>(type: "integer", nullable: false),
                    IsDispensed = table.Column<bool>(type: "boolean", nullable: false),
                    DispensedByPharmacistId = table.Column<string>(type: "text", nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Users_DispensedByPharmacistId",
                        column: x => x.DispensedByPharmacistId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsultationId = table.Column<int>(type: "integer", nullable: false),
                    ReferredHospital = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Urgency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    MasterMedicineId = table.Column<int>(type: "integer", nullable: false),
                    Dosage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    QuantityPrescribed = table.Column<int>(type: "integer", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_MasterMedicines_MasterMedicineId",
                        column: x => x.MasterMedicineId,
                        principalTable: "MasterMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonationProfiles_UserId",
                table: "BloodDonationProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CampInventories_CampId",
                table: "CampInventories",
                column: "CampId");

            migrationBuilder.CreateIndex(
                name: "IX_CampInventories_MasterMedicineId",
                table: "CampInventories",
                column: "MasterMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Camps_HostId",
                table: "Camps",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_TriageRecordId",
                table: "Consultations",
                column: "TriageRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MasterMedicineId",
                table: "PrescriptionItems",
                column: "MasterMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ConsultationId",
                table: "Prescriptions",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DispensedByPharmacistId",
                table: "Prescriptions",
                column: "DispensedByPharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ConsultationId",
                table: "Referrals",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TriageRecords_CampId",
                table: "TriageRecords",
                column: "CampId");

            migrationBuilder.CreateIndex(
                name: "IX_TriageRecords_PatientId",
                table: "TriageRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TriageRecords_VolunteerId",
                table: "TriageRecords",
                column: "VolunteerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodDonationProfiles");

            migrationBuilder.DropTable(
                name: "CampInventories");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "MasterMedicines");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "TriageRecords");

            migrationBuilder.DropTable(
                name: "Camps");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
