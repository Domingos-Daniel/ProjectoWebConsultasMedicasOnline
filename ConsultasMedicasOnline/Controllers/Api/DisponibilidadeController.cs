using System;
using System.Collections.Generic;
using System.Linq;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultasMedicasOnline.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilidadeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DisponibilidadeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Disponibilidade/Medico/5
        [HttpGet("Medico/{id}")]
        public ActionResult GetMedicoDisponibilidade(int id)
        {
            try
            {
                // Verificar se o médico existe
                var medico = _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidade)
                    .FirstOrDefault(m => m.Id == id);
                    
                if (medico == null)
                {
                    return NotFound(new { erro = "Médico não encontrado", sucesso = false });
                }

                // Simular dados de disponibilidade por enquanto
                var datasDisponiveis = new List<object>();
                var startDate = DateTime.Today;
                
                // Gerar próximos 30 dias úteis
                for (int i = 1; i <= 30; i++)
                {
                    var data = startDate.AddDays(i);
                    
                    // Pular domingos
                    if (data.DayOfWeek == DayOfWeek.Sunday)
                        continue;
                    
                    var horarios = new List<string>();
                    
                    // Manhã: 8:00 - 11:30
                    for (int hour = 8; hour < 12; hour++)
                    {
                        horarios.Add($"{hour:D2}:00");
                        horarios.Add($"{hour:D2}:30");
                    }
                    
                    // Tarde: 14:00 - 17:30 (exceto sábado)
                    if (data.DayOfWeek != DayOfWeek.Saturday)
                    {
                        for (int hour = 14; hour < 18; hour++)
                        {
                            horarios.Add($"{hour:D2}:00");
                            horarios.Add($"{hour:D2}:30");
                        }
                    }
                    
                    datasDisponiveis.Add(new
                    {
                        data = data.ToString("yyyy-MM-dd"),
                        horariosDisponiveis = horarios,
                        diaSemana = data.ToString("dddd", new System.Globalization.CultureInfo("pt-BR")),
                        totalHorarios = horarios.Count
                    });
                }

                var response = new
                {
                    sucesso = true,
                    medicoId = id,
                    nomeMedico = $"Dr. {medico.Usuario.Nome} {medico.Usuario.Sobrenome}",
                    especialidade = medico.Especialidade?.Nome ?? "Não informado",
                    crm = medico.CRM ?? "Não informado",
                    valorConsulta = medico.ValorConsulta?.ToString("C") ?? "A consultar",
                    disponibilidade = datasDisponiveis,
                    totalDatasDisponiveis = datasDisponiveis.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    erro = "Erro interno do servidor", 
                    detalhes = ex.Message,
                    sucesso = false 
                });
            }
        }

        // GET: api/Disponibilidade/Medico/{id}/Data/{data}
        [HttpGet("Medico/{id}/Data/{data}")]
        public IActionResult GetHorariosData(int id, string data)
        {
            try
            {
                var dataConsulta = DateTime.ParseExact(data, "yyyy-MM-dd", null);
                
                var medico = _context.Medicos.Find(id);
                if (medico == null)
                {
                    return NotFound(new { erro = "Médico não encontrado" });
                }

                // Get exact start and end of the date to avoid timezone issues
                var startOfDay = dataConsulta.Date; 
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                // Buscar consultas já agendadas para esta data - add exact date comparison
                var consultasAgendadas = _context.Consultas
                    .Where(c => c.MedicoId == id && 
                          c.DataHora >= startOfDay &&
                          c.DataHora <= endOfDay &&
                          (c.Status == StatusConsulta.Agendada || c.Status == StatusConsulta.Confirmada))
                    .Select(c => new { 
                        Horario = c.DataHora.ToString("HH:mm"),
                        Duracao = c.DuracaoMinutos
                    })
                    .ToList();

                // Debug - log what appointments were found for this date
                Console.WriteLine($"Found {consultasAgendadas.Count} bookings on {data} for doctor {id}");
                foreach (var booking in consultasAgendadas)
                {
                    Console.WriteLine($"Booked time: {booking.Horario}, Duration: {booking.Duracao}");
                }

                // Gerar todos os horários (disponíveis e ocupados)
                var todosHorarios = new List<object>();
                
                // Manhã
                for (int hour = 8; hour < 12; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 30)
                    {
                        var horario = $"{hour:D2}:{minute:D2}";
                        var ocupado = consultasAgendadas.Any(c => c.Horario == horario);
                        
                        todosHorarios.Add(new {
                            horario = horario,
                            disponivel = !ocupado,
                            periodo = "manha"
                        });
                    }
                }
                
                // Tarde (exceto sábado)
                if (dataConsulta.DayOfWeek != DayOfWeek.Saturday)
                {
                    for (int hour = 14; hour < 18; hour++)
                    {
                        for (int minute = 0; minute < 60; minute += 30)
                        {
                            var horario = $"{hour:D2}:{minute:D2}";
                            var ocupado = consultasAgendadas.Any(c => c.Horario == horario);
                            
                            todosHorarios.Add(new {
                                horario = horario,
                                disponivel = !ocupado,
                                periodo = "tarde"
                            });
                        }
                    }
                }

                // Extrair apenas os horários disponíveis para manter compatibilidade
                var horariosDisponiveis = todosHorarios
                    .Where(h => (bool)((dynamic)h).disponivel)
                    .Select(h => ((dynamic)h).horario.ToString())
                    .ToList();

                return Ok(new { 
                    data = data,
                    diaSemana = dataConsulta.ToString("dddd", new System.Globalization.CultureInfo("pt-BR")),
                    horariosDisponiveis = horariosDisponiveis,
                    totalHorarios = horariosDisponiveis.Count,
                    todosHorarios = todosHorarios // Incluir todos os horários, com flag de disponibilidade
                });
            }
            catch (FormatException)
            {
                return BadRequest(new { erro = "Formato de data inválido. Use yyyy-MM-dd" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHorariosData: {ex.Message}");
                return StatusCode(500, new { erro = "Erro interno do servidor", detalhes = ex.Message });
            }
        }
    }
}
