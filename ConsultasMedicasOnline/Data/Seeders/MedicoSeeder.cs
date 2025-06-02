using ConsultasMedicasOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsultasMedicasOnline.Data.Seeders
{
    public class MedicoSeeder
    {
        public static async Task SeedMedicos(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            // Check if any doctors exist
            if (await context.Medicos.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Create specialties if they don't exist
            if (!await context.Especialidades.AnyAsync())
            {
                var especialidades = new List<Especialidade>
                {
                    new Especialidade
                    {
                        Nome = "Clínica Geral",
                        Descricao = "Medicina geral e familiar",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    },
                    new Especialidade
                    {
                        Nome = "Cardiologia",
                        Descricao = "Tratamento de doenças do coração",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    },
                    new Especialidade
                    {
                        Nome = "Dermatologia",
                        Descricao = "Tratamento de problemas de pele",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    },
                    new Especialidade
                    {
                        Nome = "Pediatria",
                        Descricao = "Cuidados médicos para crianças",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    }
                };

                context.Especialidades.AddRange(especialidades);
                await context.SaveChangesAsync();
            }

            // Get specialties for the doctors
            var clinicaGeral = await context.Especialidades.FirstOrDefaultAsync(e => e.Nome == "Clínica Geral");
            var cardiologia = await context.Especialidades.FirstOrDefaultAsync(e => e.Nome == "Cardiologia");

            if (clinicaGeral == null || cardiologia == null)
            {
                // If specialties were not found, create them directly
                if (clinicaGeral == null)
                {
                    clinicaGeral = new Especialidade
                    {
                        Nome = "Clínica Geral",
                        Descricao = "Medicina geral e familiar",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    };
                    context.Especialidades.Add(clinicaGeral);
                }
                
                if (cardiologia == null)
                {
                    cardiologia = new Especialidade
                    {
                        Nome = "Cardiologia",
                        Descricao = "Tratamento de doenças do coração",
                        Ativa = true,
                        DataCriacao = DateTime.Now
                    };
                    context.Especialidades.Add(cardiologia);
                }
                
                await context.SaveChangesAsync();
            }

            // Create doctor users
            var drRobertoUser = new Usuario
            {
                UserName = "dr.roberto@clinica.ao",
                Email = "dr.roberto@clinica.ao",
                Nome = "Roberto",
                Sobrenome = "Silva",
                EmailConfirmed = true,
                PhoneNumber = "+244923456789",
                PhoneNumberConfirmed = true,
                DataCriacao = DateTime.Now,
                CPF = "12345678901",
                Ativo = true
            };

            var drAnaUser = new Usuario
            {
                UserName = "dra.ana@clinica.ao",
                Email = "dra.ana@clinica.ao",
                Nome = "Ana",
                Sobrenome = "Martins",
                EmailConfirmed = true,
                PhoneNumber = "+244934567890",
                PhoneNumberConfirmed = true,
                DataCriacao = DateTime.Now,
                CPF = "98765432109",
                Ativo = true
            };

            // Check if users already exist
            var existingDrRoberto = await userManager.FindByEmailAsync(drRobertoUser.Email);
            var existingDrAna = await userManager.FindByEmailAsync(drAnaUser.Email);

            // Create users if they don't exist
            if (existingDrRoberto == null)
            {
                var result = await userManager.CreateAsync(drRobertoUser, "Medico@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(drRobertoUser, "Medico");
                }
            }
            else
            {
                drRobertoUser = existingDrRoberto;
            }

            if (existingDrAna == null)
            {
                var result = await userManager.CreateAsync(drAnaUser, "Medico@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(drAnaUser, "Medico");
                }
            }
            else
            {
                drAnaUser = existingDrAna;
            }

            // Make sure the database has been updated before proceeding
            await context.SaveChangesAsync();

            // Create doctor profiles
            var medicos = new List<Medico>
            {
                new Medico
                {
                    UsuarioId = drRobertoUser.Id,
                    EspecialidadeId = clinicaGeral.Id,
                    CRM = "12345-AO",
                    EstadoCRM = "Luanda",
                    Biografia = "Médico experiente com mais de 10 anos de prática clínica. Especialista em medicina familiar e preventiva.",
                    ValorConsulta = 15000, // 15.000 Kz
                    TempoConsultaMinutos = 30,
                    AceitaConvenio = true,
                    AceitaParticular = true,
                    DataCadastro = DateTime.Now
                },
                new Medico
                {
                    UsuarioId = drAnaUser.Id,
                    EspecialidadeId = cardiologia.Id,
                    CRM = "67890-AO",
                    EstadoCRM = "Luanda",
                    Biografia = "Cardiologista formada pela Universidade de Lisboa, com especialização em cardiologia preventiva e tratamento de hipertensão.",
                    ValorConsulta = 25000, // 25.000 Kz
                    TempoConsultaMinutos = 45,
                    AceitaConvenio = true,
                    AceitaParticular = true,
                    DataCadastro = DateTime.Now
                }
            };

            // Add doctors to database
            context.Medicos.AddRange(medicos);
            await context.SaveChangesAsync();
        }
    }
}
