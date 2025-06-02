using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultasMedicasOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddPagamentoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Pacientes_PacienteId",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_HorariosDisponiveis_Medicos_MedicoId",
                table: "HorariosDisponiveis");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagamentos_Consultas_ConsultaId",
                table: "Pagamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Pagamentos_PagamentoId",
                table: "Transacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transacoes",
                table: "Transacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagamentos",
                table: "Pagamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorariosDisponiveis",
                table: "HorariosDisponiveis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enderecos",
                table: "Enderecos");

            migrationBuilder.RenameTable(
                name: "Transacoes",
                newName: "Transacao");

            migrationBuilder.RenameTable(
                name: "Pagamentos",
                newName: "Pagamento");

            migrationBuilder.RenameTable(
                name: "HorariosDisponiveis",
                newName: "HorarioDisponivel");

            migrationBuilder.RenameTable(
                name: "Enderecos",
                newName: "Endereco");

            migrationBuilder.RenameIndex(
                name: "IX_Transacoes_PagamentoId",
                table: "Transacao",
                newName: "IX_Transacao_PagamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamentos_ConsultaId",
                table: "Pagamento",
                newName: "IX_Pagamento_ConsultaId");

            migrationBuilder.RenameIndex(
                name: "IX_HorariosDisponiveis_MedicoId",
                table: "HorarioDisponivel",
                newName: "IX_HorarioDisponivel_MedicoId");

            migrationBuilder.RenameIndex(
                name: "IX_Enderecos_PacienteId",
                table: "Endereco",
                newName: "IX_Endereco_PacienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transacao",
                table: "Transacao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagamento",
                table: "Pagamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorarioDisponivel",
                table: "HorarioDisponivel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Endereco",
                table: "Endereco",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_Pacientes_PacienteId",
                table: "Endereco",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorarioDisponivel_Medicos_MedicoId",
                table: "HorarioDisponivel",
                column: "MedicoId",
                principalTable: "Medicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamento_Consultas_ConsultaId",
                table: "Pagamento",
                column: "ConsultaId",
                principalTable: "Consultas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Pagamento_PagamentoId",
                table: "Transacao",
                column: "PagamentoId",
                principalTable: "Pagamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_Pacientes_PacienteId",
                table: "Endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_HorarioDisponivel_Medicos_MedicoId",
                table: "HorarioDisponivel");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagamento_Consultas_ConsultaId",
                table: "Pagamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Pagamento_PagamentoId",
                table: "Transacao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transacao",
                table: "Transacao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagamento",
                table: "Pagamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorarioDisponivel",
                table: "HorarioDisponivel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Endereco",
                table: "Endereco");

            migrationBuilder.RenameTable(
                name: "Transacao",
                newName: "Transacoes");

            migrationBuilder.RenameTable(
                name: "Pagamento",
                newName: "Pagamentos");

            migrationBuilder.RenameTable(
                name: "HorarioDisponivel",
                newName: "HorariosDisponiveis");

            migrationBuilder.RenameTable(
                name: "Endereco",
                newName: "Enderecos");

            migrationBuilder.RenameIndex(
                name: "IX_Transacao_PagamentoId",
                table: "Transacoes",
                newName: "IX_Transacoes_PagamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamento_ConsultaId",
                table: "Pagamentos",
                newName: "IX_Pagamentos_ConsultaId");

            migrationBuilder.RenameIndex(
                name: "IX_HorarioDisponivel_MedicoId",
                table: "HorariosDisponiveis",
                newName: "IX_HorariosDisponiveis_MedicoId");

            migrationBuilder.RenameIndex(
                name: "IX_Endereco_PacienteId",
                table: "Enderecos",
                newName: "IX_Enderecos_PacienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transacoes",
                table: "Transacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagamentos",
                table: "Pagamentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorariosDisponiveis",
                table: "HorariosDisponiveis",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enderecos",
                table: "Enderecos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Pacientes_PacienteId",
                table: "Enderecos",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosDisponiveis_Medicos_MedicoId",
                table: "HorariosDisponiveis",
                column: "MedicoId",
                principalTable: "Medicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamentos_Consultas_ConsultaId",
                table: "Pagamentos",
                column: "ConsultaId",
                principalTable: "Consultas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Pagamentos_PagamentoId",
                table: "Transacoes",
                column: "PagamentoId",
                principalTable: "Pagamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
