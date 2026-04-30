using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIComplaintAgent.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerResponse",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerResponse",
                table: "Complaints");
        }
    }
}
