using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using ConsultasMedicasOnline.Services;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize]
    public class ProntuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;  // Alterado para usar EmailService diretamente

        public ProntuariosController(ApplicationDbContext context, EmailService emailService) // Alterado para injetar EmailService
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Prontuarios
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var prontuarios = await _context.Prontuarios
                .Include(p => p.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(p => p.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(p => p.Consulta)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
                
            return View(prontuarios);
        }

        // GET: Prontuarios/Details/5
        [Authorize(Roles = "Administrador,Medico,Paciente")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prontuario = await _context.Prontuarios
                .Include(p => p.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(p => p.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(p => p.Consulta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prontuario == null)
            {
                return NotFound();
            }

            // Verify access permissions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool canAccess = User.IsInRole("Administrador") || 
                           (User.IsInRole("Medico") && prontuario.Medico.UsuarioId == userId) || 
                           (User.IsInRole("Paciente") && prontuario.Paciente.UsuarioId == userId);

            if (!canAccess)
            {
                return Forbid();
            }

            return View(prontuario);
        }

        // GET: Prontuarios/Create
        [Authorize(Roles = "Medico,Administrador")]
        public async Task<IActionResult> Create(int consultaId)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(c => c.Id == consultaId);
                
            if (consulta == null)
            {
                return NotFound();
            }
            
            // Check if prontuário already exists
            var prontuarioExistente = await _context.Prontuarios
                .FirstOrDefaultAsync(p => p.ConsultaId == consultaId);
                
            if (prontuarioExistente != null)
            {
                return RedirectToAction("Edit", new { id = prontuarioExistente.Id });
            }
            
            // Create a new prontuário
            var prontuario = new Prontuario
            {
                ConsultaId = consulta.Id,
                PacienteId = consulta.PacienteId,
                MedicoId = consulta.MedicoId,
                DataCriacao = DateTime.Now
            };
            
            ViewData["Consulta"] = consulta;
            return View(prontuario);
        }
        
        // POST: Prontuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Medico,Administrador")]
        public async Task<IActionResult> Create(Prontuario prontuario)
        {
            // Debug para verificar os valores recebidos
            Console.WriteLine($"ConsultaId: {prontuario.ConsultaId}, PacienteId: {prontuario.PacienteId}, MedicoId: {prontuario.MedicoId}");
            
            // Garantir que a data de criação seja definida
            prontuario.DataCriacao = DateTime.Now;
            
            try
            {
                // Verificar se diagnóstico está preenchido (validação manual)
                if (string.IsNullOrWhiteSpace(prontuario.Diagnostico))
                {
                    ModelState.AddModelError("Diagnostico", "O diagnóstico é obrigatório");
                    throw new Exception("O campo diagnóstico é obrigatório.");
                }
                
                // Importante: Carregar explicitamente as entidades relacionadas antes de adicionar o prontuário
                // Isso garante que as propriedades de navegação estejam preenchidas
                var consulta = await _context.Consultas.FindAsync(prontuario.ConsultaId);
                var paciente = await _context.Pacientes
                    .Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.Id == prontuario.PacienteId);
                var medico = await _context.Medicos
                    .Include(m => m.Usuario)
                    .FirstOrDefaultAsync(m => m.Id == prontuario.MedicoId);
                
                if (consulta == null || paciente == null || medico == null)
                {
                    if (consulta == null) ModelState.AddModelError("ConsultaId", "Consulta não encontrada");
                    if (paciente == null) ModelState.AddModelError("PacienteId", "Paciente não encontrado");
                    if (medico == null) ModelState.AddModelError("MedicoId", "Médico não encontrado");
                    throw new Exception("Uma ou mais entidades relacionadas não foram encontradas.");
                }
                
                // Importante: Use uma abordagem diferente com Entry/State para evitar problemas de tracking
                // Crie um novo prontuário com todos os valores e entidades relacionadas
                var novoProntuario = new Prontuario
                {
                    ConsultaId = prontuario.ConsultaId,
                    PacienteId = prontuario.PacienteId,
                    MedicoId = prontuario.MedicoId,
                    DataCriacao = prontuario.DataCriacao,
                    Diagnostico = prontuario.Diagnostico,
                    ExameFisico = prontuario.ExameFisico,
                    ExamesSolicitados = prontuario.ExamesSolicitados,
                    Hipoteses = prontuario.Hipoteses,
                    HistoriaClinica = prontuario.HistoriaClinica,
                    OrientacoesGerais = prontuario.OrientacoesGerais,
                    Prescricoes = prontuario.Prescricoes,
                    ProximaConsulta = prontuario.ProximaConsulta,
                    Tratamento = prontuario.Tratamento,
                    // Definir as propriedades de navegação
                    Consulta = consulta,
                    Paciente = paciente,
                    Medico = medico
                };
                
                // Adicionar o novo prontuário
                _context.Prontuarios.Add(novoProntuario);
                await _context.SaveChangesAsync();
                
                // Atualizar o status da consulta
                if (consulta.Status != StatusConsulta.Concluida)
                {
                    consulta.Status = StatusConsulta.Concluida;
                    _context.Update(consulta);
                    await _context.SaveChangesAsync();
                }
                
                // Enviar email ao paciente sobre o novo prontuário
                try
                {
                    var pacienteEmail = paciente.Usuario?.Email;
                    var pacienteNome = $"{paciente.Usuario?.Nome} {paciente.Usuario?.Sobrenome}";
                    var medicoNome = $"Dr. {medico.Usuario?.Nome} {medico.Usuario?.Sobrenome}";
                    
                    if (!string.IsNullOrEmpty(pacienteEmail))
                    {
                        // Agora podemos enviar o email sem erros porque estamos usando diretamente o EmailService
                        await _emailService.SendEmailAsync(
                            pacienteEmail, 
                            "Seu prontuário médico foi criado",
                            $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                                <div style='text-align: center; padding: 15px; background-color: #f0f9ff; border-radius: 5px;'>
                                    <h1 style='color: #1e40af; margin: 0;'>MedConsulta</h1>
                                    <p style='color: #64748b; margin: 5px 0 0;'>Sistema de Consultas Médicas Online</p>
                                </div>
                                
                                <div style='padding: 20px;'>
                                    <p>Olá, <strong>{pacienteNome}</strong>!</p>
                                    
                                    <p>O <strong>{medicoNome}</strong> concluiu o seu prontuário médico da consulta realizada em <strong>{consulta.DataHora.ToString("dd/MM/yyyy")}</strong>.</p>
                                    
                                    <p>Você pode acessar seu prontuário completo através do link abaixo:</p>
                                    
                                    <div style='text-align: center; margin: 25px 0;'>
                                        <a href='{Request.Scheme}://{Request.Host}/Prontuarios/Details/{novoProntuario.Id}' 
                                           style='background-color: #1e40af; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                            Visualizar Prontuário
                                        </a>
                                    </div>
                                    
                                    <div style='margin-top: 30px; padding: 15px; background-color: #f0f9ff; border-radius: 5px;'>
                                        <p style='margin: 0; color: #1e40af;'><strong>Importante:</strong> O prontuário contém informações importantes sobre seu diagnóstico, tratamentos prescritos e orientações médicas.</p>
                                    </div>
                                </div>
                                
                                <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                                    <p style='color: #64748b; font-size: 14px;'>© {DateTime.Now.Year} MedConsulta - Sistema de Consultas Médicas Online</p>
                                </div>
                            </div>
                            ");
                        
                        Console.WriteLine($"Email de prontuário enviado para {pacienteEmail}");
                    }
                }
                catch (Exception emailEx)
                {
                    // Log do erro de envio de email, mas não interrompemos o fluxo principal
                    Console.WriteLine($"Erro ao enviar email do prontuário: {emailEx.Message}");
                }
                
                TempData["Success"] = "Prontuário criado com sucesso.";
                return RedirectToAction("Details", "Consultas", new { id = prontuario.ConsultaId });
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"Erro ao criar prontuário: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", "Ocorreu um erro ao salvar o prontuário: " + ex.Message);
                
                // Log dos erros de validação
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Erro de validação: {error.ErrorMessage}");
                    }
                }
                
                // Recarregar a consulta para a view
                var consultaInfo = await _context.Consultas
                    .Include(c => c.Medico)
                        .ThenInclude(m => m.Usuario)
                    .Include(c => c.Paciente)
                        .ThenInclude(p => p.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == prontuario.ConsultaId);
                    
                ViewData["Consulta"] = consultaInfo;
                TempData["Error"] = "Houve um problema ao salvar o prontuário. Por favor, verifique os campos preenchidos.";
                return View(prontuario);
            }
        }

        // GET: Prontuarios/Edit/5
        [Authorize(Roles = "Medico,Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prontuario = await _context.Prontuarios
                .Include(p => p.Consulta)
                    .ThenInclude(c => c.Paciente)
                        .ThenInclude(p => p.Usuario)
                .Include(p => p.Consulta)
                    .ThenInclude(c => c.Medico)
                        .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (prontuario == null)
            {
                return NotFound();
            }

            ViewData["Consulta"] = prontuario.Consulta;
            return View(prontuario);
        }

        // POST: Prontuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Medico,Administrador")]
        public async Task<IActionResult> Edit(int id, Prontuario prontuario)
        {
            if (id != prontuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prontuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Prontuário atualizado com sucesso.";
                    return RedirectToAction("Details", "Consultas", new { id = prontuario.ConsultaId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProntuarioExists(prontuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var consultaInfo = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == prontuario.ConsultaId);
                
            ViewData["Consulta"] = consultaInfo;
            return View(prontuario);
        }

        private bool ProntuarioExists(int id)
        {
            return _context.Prontuarios.Any(e => e.Id == id);
        }
    }
}
