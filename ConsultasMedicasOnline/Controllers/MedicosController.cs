using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize]
    public class MedicosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public MedicosController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medicos
        public async Task<IActionResult> Index()
        {
            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .ToListAsync();
            return View(medicos);
        }

        // GET: Medicos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .Include(m => m.HorariosDisponiveis)
                .Include(m => m.Consultas.Where(c => c.Status != StatusConsulta.Cancelada))
                    .ThenInclude(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null)
            {
                return NotFound();
            }

            // Se não for admin ou o próprio médico, mostrar apenas informações públicas
            if (!User.IsInRole("Administrador") && medico.UsuarioId != currentUserId)
            {
                // Limpar informações sensíveis para pacientes
                medico.Consultas = new List<Consulta>();
            }

            return View(medico);
        }

        // GET: Medicos/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome");
            return View();
        }

        // POST: Medicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("CRM,EstadoCRM,EspecialidadeId,Biografia,ValorConsulta,TempoConsultaMinutos,AceitaParticular,AceitaConvenio")] Medico medico, string UsuarioId)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(UsuarioId))
            {
                var usuario = await _userManager.FindByIdAsync(UsuarioId);
                
                if (usuario == null)
                {
                    ModelState.AddModelError("UsuarioId", "Usuário não encontrado.");
                    ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
                    return View(medico);
                }

                // Verificar se o usuário já não é médico
                var medicoExistente = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.UsuarioId == UsuarioId);
                
                if (medicoExistente != null)
                {
                    ModelState.AddModelError("", "Este usuário já está cadastrado como médico.");
                    ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
                    return View(medico);
                }

                // Verificar CRM único
                var crmExistente = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.CRM == medico.CRM && m.EstadoCRM == medico.EstadoCRM);
                
                if (crmExistente != null)
                {
                    ModelState.AddModelError("CRM", "Este CRM já está cadastrado.");
                    ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
                    return View(medico);
                }

                medico.UsuarioId = UsuarioId;
                medico.DataCadastro = DateTime.UtcNow;
                
                _context.Add(medico);
                await _context.SaveChangesAsync();

                // Adicionar role de Médico
                if (!await _userManager.IsInRoleAsync(usuario, "Medico"))
                {
                    await _userManager.AddToRoleAsync(usuario, "Medico");
                }

                return RedirectToAction(nameof(Details), new { id = medico.Id });
            }
            
            ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
            return View(medico);
        }

        // GET: Medicos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var medico = await _context.Medicos.FindAsync(id);
            
            if (medico == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode editar
            if (!User.IsInRole("Administrador") && medico.UsuarioId != currentUserId)
            {
                return Forbid();
            }

            ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
            return View(medico);
        }

        // POST: Medicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,CRM,EstadoCRM,EspecialidadeId,Biografia,ValorConsulta,TempoConsultaMinutos,AceitaParticular,AceitaConvenio,DataCadastro")] Medico medico)
        {
            if (id != medico.Id)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            
            // Verificar se o usuário pode editar
            if (!User.IsInRole("Administrador") && medico.UsuarioId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar CRM único (excluindo o próprio registro)
                    var crmExistente = await _context.Medicos
                        .FirstOrDefaultAsync(m => m.CRM == medico.CRM && 
                                                 m.EstadoCRM == medico.EstadoCRM && 
                                                 m.Id != medico.Id);
                    
                    if (crmExistente != null)
                    {
                        ModelState.AddModelError("CRM", "Este CRM já está cadastrado.");
                        ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
                        return View(medico);
                    }

                    _context.Update(medico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicoExists(medico.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = medico.Id });
            }
            
            ViewData["EspecialidadeId"] = new SelectList(_context.Especialidades, "Id", "Nome", medico.EspecialidadeId);
            return View(medico);
        }

        // GET: Medicos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (medico == null)
            {
                return NotFound();
            }

            return View(medico);
        }

        // POST: Medicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico != null)
            {
                _context.Medicos.Remove(medico);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MedicoExists(int id)
        {
            return _context.Medicos.Any(e => e.Id == id);
        }

        // GET: Medicos/MeuPerfil
        [Authorize(Roles = "Medico")]
        public async Task<IActionResult> MeuPerfil()
        {
            var currentUserId = _userManager.GetUserId(User);
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .Include(m => m.HorariosDisponiveis)
                .FirstOrDefaultAsync(m => m.UsuarioId == currentUserId);

            if (medico == null)
            {
                return NotFound("Perfil de médico não encontrado. Entre em contato com o administrador.");
            }

            return View("Details", medico);
        }

        // GET: Medicos/PorEspecialidade/5
        public async Task<IActionResult> PorEspecialidade(int especialidadeId)
        {
            var especialidade = await _context.Especialidades.FindAsync(especialidadeId);
            if (especialidade == null)
            {
                return NotFound();
            }

            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .Where(m => m.EspecialidadeId == especialidadeId)
                .ToListAsync();

            ViewBag.Especialidade = especialidade.Nome;
            return View("Index", medicos);
        }
    }
}
