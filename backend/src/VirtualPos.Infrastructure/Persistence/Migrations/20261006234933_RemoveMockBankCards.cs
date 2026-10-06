using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMockBankCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockBankCards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MockBankCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CardHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MaskedCardNumber = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockBankCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockBankCards_CardHash",
                table: "MockBankCards",
                column: "CardHash",
                unique: true);
        }
    }
}
