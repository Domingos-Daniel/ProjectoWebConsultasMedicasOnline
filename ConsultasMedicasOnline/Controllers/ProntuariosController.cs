using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using ConsultasMedicasOnline.Services;
using System.Security.Claims;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize]
    public class ProntuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ProntuariosController(ApplicationDbContext context, IEmailService emailService)
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
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> Create(int? consultaId)
        {
            if (consultaId == null)
            {
                return BadRequest("ID da consulta é necessário");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == consultaId);

            if (consulta == null)
            {
                return NotFound("Consulta não encontrada");
            }

            // Verify if the current user is the doctor of this consultation or an admin
            bool canCreate = User.IsInRole("Administrador") || 
                            (User.IsInRole("Medico") && consulta.Medico.UsuarioId == userId);

            if (!canCreate)
            {
                return Forbid();
            }

            // Check if the consultation already has a prontuario
            if (await _context.Prontuarios.AnyAsync(p => p.ConsultaId == consulta.Id))
            {
                TempData["Error"] = "Esta consulta já possui um prontuário.";
                return RedirectToAction("Details", "Consultas", new { id = consulta.Id });
            }

            // Create viewmodel with pre-filled data
            var prontuario = new Prontuario
            {
                ConsultaId = consulta.Id,
                PacienteId = consulta.PacienteId,
                MedicoId = consulta.MedicoId,
                DataCriacao = DateTime.UtcNow
            };

            ViewData["Consulta"] = consulta;
            ViewData["Paciente"] = consulta.Paciente;
            ViewData["Medico"] = consulta.Medico;
            
            return View(prontuario);
        }

        // POST: Prontuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> Create(
            [Bind("ConsultaId,PacienteId,MedicoId,HistoriaClinica,ExameFisico,Hipoteses,Diagnostico,Tratamento,Prescricoes,ExamesSolicitados,OrientacoesGerais,ProximaConsulta")] Prontuario prontuario)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                .FirstOrDefaultAsync(c => c.Id == prontuario.ConsultaId);

            if (consulta == null)
            {
                return NotFound("Consulta não encontrada");
            }

            // Verify if the current user is the doctor of this consultation or an admin
            bool canCreate = User.IsInRole("Administrador") || 
                           (User.IsInRole("Medico") && consulta.Medico.UsuarioId == userId);

            if (!canCreate)
            {
                return Forbid();
            }
            
            // Set creation date
            prontuario.DataCriacao = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(prontuario);
                
                // Mark the consultation as completed if it's in progress
                if (consulta.Status == StatusConsulta.EmAndamento)
                {
                    consulta.Status = StatusConsulta.Concluida;
                    _context.Update(consulta);
                }
                
                await _context.SaveChangesAsync();
                
                // Send email notification to the patient
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Paciente.Usuario.Email,
                            "Prontuário médico registrado",
                            GetProntuarioEmailBody(
                                consulta.Paciente.Usuario.Nome,
                                consulta.Medico.Usuario.Nome + " " + consulta.Medico.Usuario.Sobrenome,
                                consulta.DataHora
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't stop the process
                        Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                    }
                }
                
                TempData["Success"] = "Prontuário registrado com sucesso!";
                return RedirectToAction("Details", "Consultas", new { id = prontuario.ConsultaId });
            }
            
            // If we got this far, something failed, redisplay form
            ViewData["Consulta"] = consulta;
            ViewData["Paciente"] = consulta.Paciente;
            ViewData["Medico"] = consulta.Medico;
            
            return View(prontuario);
        }

        // GET: Prontuarios/Edit/5
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prontuario = await _context.Prontuarios
                .Include(p => p.Consulta)
                .Include(p => p.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(p => p.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (prontuario == null)
            {
                return NotFound();
            }

            // Verify if the current user is the doctor of this prontuario or an admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool canEdit = User.IsInRole("Administrador") || 
                          (User.IsInRole("Medico") && prontuario.Medico.UsuarioId == userId);

            if (!canEdit)
            {
                return Forbid();
            }

            ViewData["Consulta"] = prontuario.Consulta;
            ViewData["Paciente"] = prontuario.Paciente;
            ViewData["Medico"] = prontuario.Medico;
            
            return View(prontuario);
        }

        // POST: Prontuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> Edit(int id, 
            [Bind("Id,ConsultaId,PacienteId,MedicoId,HistoriaClinica,ExameFisico,Hipoteses,Diagnostico,Tratamento,Prescricoes,ExamesSolicitados,OrientacoesGerais,ProximaConsulta,DataCriacao")] Prontuario prontuario)
        {
            if (id != prontuario.Id)
            {
                return NotFound();
            }

            // Verify if the current user is the doctor of this prontuario or an admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.Id == prontuario.MedicoId);
            
            if (medico == null)
            {
                return NotFound("Médico não encontrado");
            }
            
            bool canEdit = User.IsInRole("Administrador") || 
                          (User.IsInRole("Medico") && medico.UsuarioId == userId);

            if (!canEdit)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prontuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Prontuário atualizado com sucesso!";
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
                return RedirectToAction("Details", new { id = prontuario.Id });
            }
            
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == prontuario.ConsultaId);
                
            ViewData["Consulta"] = consulta;
            ViewData["Paciente"] = consulta.Paciente;
            ViewData["Medico"] = consulta.Medico;
            
            return View(prontuario);
        }

        // GET: Prontuarios/PatientRecords/5
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> PatientRecords(int? id)
        {
            if (id == null)
            {
                return BadRequest("ID do paciente é necessário");
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
            {
                return NotFound("Paciente não encontrado");
            }

            // If the user is a doctor, verify that they have treated this patient
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Medico"))
            {
                var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
                if (medico == null)
                {
                    return NotFound("Perfil médico não encontrado");
                }
                
                bool hasTreatedPatient = await _context.Consultas
                    .AnyAsync(c => c.MedicoId == medico.Id && c.PacienteId == paciente.Id && 
                    (c.Status == StatusConsulta.Concluida || c.Status == StatusConsulta.EmAndamento));
                
                if (!hasTreatedPatient)
                {
                    return Forbid();
                }
            }

            var prontuarios = await _context.Prontuarios
                .Include(p => p.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(p => p.Consulta)
                .Where(p => p.PacienteId == id)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
                
            ViewData["Paciente"] = paciente;
            return View(prontuarios);
        }

        // GET: Prontuarios/DoctorRecords/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DoctorRecords(int? id)
        {
            if (id == null)
            {
                return BadRequest("ID do médico é necessário");
            }

            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null)
            {
                return NotFound("Médico não encontrado");
            }

            var prontuarios = await _context.Prontuarios
                .Include(p => p.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(p => p.Consulta)
                .Where(p => p.MedicoId == id)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
                
            ViewData["Medico"] = medico;
            return View(prontuarios);
        }
        
        // GET: Prontuarios/Print/5
        public async Task<IActionResult> Print(int? id)
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
                .Include(p => p.Medico)
                    .ThenInclude(m => m.Especialidade)
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

        private bool ProntuarioExists(int id)
        {
            return _context.Prontuarios.Any(e => e.Id == id);
        }
        
        private string GetProntuarioEmailBody(string pacienteName, string doctorName, DateTime appointmentDate)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='text-align: center; padding: 10px; background-color: #eff6ff; border-radius: 5px;'>
                        <h2 style='color: #1e40af;'>Prontuário Médico Disponível</h2>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <p>Olá, <strong>{pacienteName}</strong>!</p>
                        
                        <p>O Dr. {doctorName} registrou seu prontuário médico referente à consulta realizada em {appointmentDate.ToString("dd/MM/yyyy")}.</p>
                        
                        <p>Você pode acessar estas informações a qualquer momento em seu painel de controle no sistema.</p>
                        
                        <div style='margin-top: 30px; padding: 15px; background-color: #f0f9ff; border-radius: 5px;'>
                            <p style='margin: 0; color: #0369a1;'><strong>Importante:</strong> Este documento contém informações médicas confidenciais. Caso tenha alguma dúvida sobre as informações registradas, entre em contato com seu médico.</p>
                        </div>
                    </div>
                    
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                        <p style='color: #64748b; font-size: 14px;'>Este é um e-mail automático. Por favor, não responda diretamente.</p>
                        <p style='color: #64748b; font-size: 14px;'>© {DateTime.Now.Year} Consultas Médicas Online - Todos os direitos reservados.</p>
                    </div>
                </div>
            ";
        }
    }
}
