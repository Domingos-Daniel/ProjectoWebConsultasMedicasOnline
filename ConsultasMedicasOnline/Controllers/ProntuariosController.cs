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
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(prontuario);
                    await _context.SaveChangesAsync();
                    
                    // Update consulta status if not already done
                    var consulta = await _context.Consultas.FindAsync(prontuario.ConsultaId);
                    if (consulta != null && consulta.Status != StatusConsulta.Concluida)
                    {
                        consulta.Status = StatusConsulta.Concluida;
                        _context.Update(consulta);
                        await _context.SaveChangesAsync();
                    }
                    
                    TempData["Success"] = "Prontuário criado com sucesso.";
                    return RedirectToAction("Details", "Consultas", new { id = prontuario.ConsultaId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocorreu um erro ao salvar o prontuário: " + ex.Message);
                }
            }
            
            // If we get this far, something failed, redisplay form
            var consultaInfo = await _context.Consultas
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(c => c.Id == prontuario.ConsultaId);
                
            ViewData["Consulta"] = consultaInfo;
            return View(prontuario);
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
           