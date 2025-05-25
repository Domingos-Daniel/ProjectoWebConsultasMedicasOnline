using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultasMedicasOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddPacientePropertiesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Pacientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Pacientes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Pacientes",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroIdentificacao",
                table: "Pacientes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Pacientes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "NumeroIdentificacao",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Pacientes");
        }
    }
}
