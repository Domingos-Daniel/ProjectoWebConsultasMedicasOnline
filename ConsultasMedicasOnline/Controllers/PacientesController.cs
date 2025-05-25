using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using System.Security.Claims;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize]
    public class PacientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ILogger<PacientesController> _logger;

        public PacientesController(ApplicationDbContext context, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ILogger<PacientesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: Pacientes
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var pacientes = await _context.Pacientes
                .Include(p => p.Usuario)
                .Include(p => p.Endereco)
                .ToListAsync();
            return View(pacientes);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);
            
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .Include(p => p.Endereco)
                .Include(p => p.Consultas)
                    .ThenInclude(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(p => p.Consultas)
                    .ThenInclude(c => c.Medico)
                    .ThenInclude(m => m.Especialidade)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode ver os detalhes
            if (!User.IsInRole("Administrador") && 
                !User.IsInRole("Medico") && 
                paciente.UsuarioId != currentUserId)
            {
                return Forbid();
            }

            return View(paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumeroIdentificacao,Telefone,TipoSanguineo,Alergias,MedicamentosEmUso,HistoricoFamiliar,ContatoEmergencia,TelefoneEmergencia")] Paciente paciente)
        {
            // Log received data for debugging
            Console.WriteLine($"Received: NIF = {paciente.NumeroIdentificacao}, Telefone = {paciente.Telefone}");
            
            // Clear model state if previously had errors
            ModelState.Clear();
            
            // Validate required fields explicitly
            if (string.IsNullOrWhiteSpace(paciente.NumeroIdentificacao))
            {
                ModelState.AddModelError("NumeroIdentificacao", "O número de identificação é obrigatório.");
            }
            
            if (string.IsNullOrWhiteSpace(paciente.Telefone))
            {
                ModelState.AddModelError("Telefone", "O telefone é obrigatório.");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = _userManager.GetUserId(User);
                    var usuario = await _userManager.FindByIdAsync(currentUserId);
                    
                    if (usuario == null)
                    {
                        return NotFound();
                    }

                    // Verificar se o usuário já não é paciente
                    var pacienteExistente = await _context.Pacientes
                        .FirstOrDefaultAsync(p => p.UsuarioId == currentUserId);
                    
                    if (pacienteExistente != null)
                    {
                        ModelState.AddModelError("", "Você já está cadastrado como paciente.");
                        return View(paciente);
                    }

                    paciente.UsuarioId = currentUserId;
                    paciente.DataCadastro = DateTime.UtcNow;
                    paciente.Ativo = true;
                    
                    _context.Add(paciente);
                    await _context.SaveChangesAsync();

                    // Adicionar role de Paciente
                    if (!await _userManager.IsInRoleAsync(usuario, "Paciente"))
                    {
                        await _userManager.AddToRoleAsync(usuario, "Paciente");
                    }

                    TempData["SuccessMessage"] = "Cadastro realizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = paciente.Id });
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    Console.WriteLine($"Error saving patient: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao salvar os dados: " + ex.Message);
                }
            }
            else
            {
                // Log validation errors for debugging
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error for {state.Key}: {error.ErrorMessage}");
                    }
                }
                
                ModelState.AddModelError("", "Por favor, corrija os erros no formulário.");
            }

            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Security check - ensure user can only edit their own profile
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (paciente.UsuarioId != currentUserId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Paciente paciente)
        {
            if (id != paciente.Id)
            {
                return NotFound();
            }

            // Security check - ensure user can only edit their own profile
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (paciente.UsuarioId != currentUserId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get original entity to update only the edited properties
                    var originalPaciente = await _context.Pacientes.FindAsync(id);
                    if (originalPaciente == null)
                    {
                        return NotFound();
                    }

                    // Update editable properties
                    originalPaciente.NumeroIdentificacao = paciente.NumeroIdentificacao;
                    originalPaciente.Telefone = paciente.Telefone;
                    originalPaciente.TipoSanguineo = paciente.TipoSanguineo;
                    originalPaciente.Alergias = paciente.Alergias;
                    originalPaciente.MedicamentosEmUso = paciente.MedicamentosEmUso;
                    originalPaciente.HistoricoFamiliar = paciente.HistoricoFamiliar;
                    originalPaciente.ContatoEmergencia = paciente.ContatoEmergencia;
                    originalPaciente.TelefoneEmergencia = paciente.TelefoneEmergencia;

                    _context.Update(originalPaciente);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction(nameof(MeuPerfil));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacienteExists(paciente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(paciente);
        }

        // GET: Pacientes/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente != null)
            {
                _context.Pacientes.Remove(paciente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PacienteExists(int id)
        {
            return _context.Pacientes.Any(e => e.Id == id);
        }

        // GET: Pacientes/MeuPerfil
        public async Task<IActionResult> MeuPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = await _userManager.FindByIdAsync(userId);
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .Include(p => p.Endereco)
                .Include(p => p.Consultas)
                    .ThenInclude(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (paciente == null)
            {
                // Redirecionar para criar perfil de paciente
                return RedirectToAction(nameof(Create));
            }

            return View("MeuPerfil", paciente);
        }

        // GET: Pacientes/Configuracoes
        public async Task<IActionResult> Configuracoes()
        {
            var currentUserId = _userManager.GetUserId(User);
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.UsuarioId == currentUserId);

            if (paciente == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View("Configuracoes", paciente);
        }

        // POST: Pacientes/AtualizarConfiguracoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarConfiguracoes([Bind("Id,UsuarioId,TipoSanguineo,Alergias,MedicamentosEmUso,HistoricoFamiliar,ContatoEmergencia,TelefoneEmergencia,DataCadastro")] Paciente paciente, bool receberNotificacoesSMS = false, bool receberNotificacoesEmail = true)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (paciente.UsuarioId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paciente);
                    await _context.SaveChangesAsync();
                    
                    // Aqui você pode adicionar lógica para salvar preferências de notificação
                    TempData["SuccessMessage"] = "Configurações atualizadas com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacienteExists(paciente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Configuracoes));
            }
            return View("Configuracoes", paciente);
        }
    }
}
