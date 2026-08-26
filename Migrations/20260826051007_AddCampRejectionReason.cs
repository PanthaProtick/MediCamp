using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCamp.Migrations
{
    /// <inheritdoc />
    public partial class AddCampRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CampRejectionReason",
                table: "Camps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampRejectionReason",
                table: "Camps");
        }
    }
}
