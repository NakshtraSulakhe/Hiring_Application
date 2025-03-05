using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hiring_Application.Migrations
{
    /// <inheritdoc />
    public partial class resumemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resume",
                table: "Applicants");

            migrationBuilder.AddColumn<string>(
                name: "ResumePath",
                table: "Applicants",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumePath",
                table: "Applicants");

            migrationBuilder.AddColumn<byte[]>(
                name: "Resume",
                table: "Applicants",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
