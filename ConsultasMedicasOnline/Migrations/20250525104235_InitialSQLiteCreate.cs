using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConsultasMedicasOnline.Migrations
{
    /// <inheritdoc />
    public partial class InitialSQLiteCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sobrenome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CPF = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    TipoSanguineo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Alergias = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MedicamentosEmUso = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HistoricoFamiliar = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ContatoEmergencia = table.Column<string>(type: "TEXT", nullable: true),
                    TelefoneEmergencia = table.Column<string>(type: "TEXT", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacientes_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    CRM = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EstadoCRM = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    EspecialidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Biografia = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ValorConsulta = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TempoConsultaMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    AceitaParticular = table.Column<bool>(type: "INTEGER", nullable: false),
                    AceitaConvenio = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medicos_Especialidades_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "Especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CEP = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enderecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enderecos_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ObservacoesGerais = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Pago = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultas_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consultas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HorariosDisponiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MedicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiaSemana = table.Column<int>(type: "INTEGER", nullable: false),
                    HorarioInicio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HorarioFim = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosDisponiveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosDisponiveis_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsultaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TipoPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TransacaoId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ObservacoesPagamento = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prontuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsultaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    HistoriaClinica = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExameFisico = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Hipoteses = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Diagnostico = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Tratamento = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Prescricoes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExamesSolicitados = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OrientacoesGerais = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProximaConsulta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prontuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prontuarios_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prontuarios_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prontuarios_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PagamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransacaoExternaId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DetalhesResposta = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataTransacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Pagamentos_PagamentoId",
                        column: x => x.PagamentoId,
                        principalTable: "Pagamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Especialidades",
                columns: new[] { "Id", "Ativa", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que se ocupa do diagnóstico e tratamento das doenças que acometem o coração", "Cardiologia" },
                    { 2, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que se ocupa do diagnóstico e tratamento das doenças da pele", "Dermatologia" },
                    { 3, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica dedicada à assistência à criança e ao adolescente", "Pediatria" },
                    { 4, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que trata das doenças do sistema reprodutor feminino", "Ginecologia" },
                    { 5, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que cuida do aparelho locomotor", "Ortopedia" },
                    { 6, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que trata dos distúrbios do sistema nervoso", "Neurologia" },
                    { 7, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que se dedica ao estudo, diagnóstico e tratamento dos transtornos mentais", "Psiquiatria" },
                    { 8, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que investiga e trata as doenças relacionadas aos olhos", "Oftalmologia" },
                    { 9, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Especialidade médica que cuida do ouvido, nariz e garganta", "Otorrinolaringologia" },
                    { 10, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Atendimento médico geral e preventivo", "Clínica Geral" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CPF",
                table: "AspNetUsers",
                column: "CPF",
                unique: true,
                filter: "[CPF] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_MedicoId",
                table: "Consultas",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PacienteId",
                table: "Consultas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_PacienteId",
                table: "Enderecos",
                column: "PacienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosDisponiveis_MedicoId",
                table: "HorariosDisponiveis",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_CRM_EstadoCRM",
                table: "Medicos",
                columns: new[] { "CRM", "EstadoCRM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadeId",
                table: "Medicos",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_UsuarioId",
                table: "Medicos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_UsuarioId",
                table: "Pacientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_ConsultaId",
                table: "Pagamentos",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prontuarios_ConsultaId",
                table: "Prontuarios",
                column: "ConsultaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prontuarios_MedicoId",
                table: "Prontuarios",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prontuarios_PacienteId",
                table: "Prontuarios",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_PagamentoId",
                table: "Transacoes",
                column: "PagamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Enderecos");

            migrationBuilder.DropTable(
                name: "HorariosDisponiveis");

            migrationBuilder.DropTable(
                name: "Prontuarios");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
