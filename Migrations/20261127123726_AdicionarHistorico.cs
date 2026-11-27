
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;


namespace Pi_1ano.Migrations
{
    public partial class AdicionarHistorico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            // Tabela Historico
            migrationBuilder.CreateTable(
                name: "Historico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy",
                            MySQLValueGenerationStrategy.IdentityColumn),

                    UsuarioId = table.Column<int>(type: "int", nullable: false),

                    TipoEntidade = table.Column<string>(type: "longtext", nullable: false),

                    EntidadeId = table.Column<int>(type: "int", nullable: false),

                    Acao = table.Column<string>(type: "longtext", nullable: false),

                    Descricao = table.Column<string>(type: "longtext", nullable: true),

                    DataHora = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico", x => x.Id);

                    table.ForeignKey(
                        name: "FK_Historico_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            // Índice do Historico
            migrationBuilder.CreateIndex(
                name: "IX_Historico_UsuarioId",
                table: "Historico",
                column: "UsuarioId");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Historico");

            migrationBuilder.DropTable(name: "ItensPedido");

            migrationBuilder.DropTable(name: "Pedidos");
        }
    }
}
