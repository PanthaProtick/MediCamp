using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MediCamp.Migrations
{
    public partial class AddCampPatientRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampPatientRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampPatientRegistrations", x => x.Id);
                    table.ForeignKey("FK_CampPatientRegistrations_Camps_CampId", x => x.CampId, "Camps", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_CampPatientRegistrations_Users_PatientId", x => x.PatientId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_CampPatientRegistrations_CampId", "CampPatientRegistrations", "CampId");
            migrationBuilder.CreateIndex("IX_CampPatientRegistrations_PatientId", "CampPatientRegistrations", "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CampPatientRegistrations");
        }
    }
}