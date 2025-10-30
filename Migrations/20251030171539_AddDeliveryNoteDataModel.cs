using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Luley_Integracion_Net.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryNoteDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryNotesDataModel",
                columns: table => new
                {
                    nroRemito = table.Column<string>(type: "text", nullable: false),
                    codArticulo = table.Column<string>(type: "text", nullable: false),
                    cantidadRemitida = table.Column<int>(type: "integer", nullable: false),
                    estadoRemito = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNotesDataModel", x => new { x.nroRemito, x.codArticulo });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryNotesDataModel");
        }
    }
}
