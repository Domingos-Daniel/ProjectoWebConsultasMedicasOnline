using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using ConsultasMedicasOnline.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize]
    public class ConsultasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEmailService _emailService;

        public ConsultasController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Consultas
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            IQueryable<Consulta> query = _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidade);

            // Filter by user role
            if (User.IsInRole("Paciente"))
            {
                var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UsuarioId == userId);
                if (paciente != null)
                {
                    query = query.Where(c => c.PacienteId == paciente.Id);
                }
                else
                {
                    return View(new List<Consulta>());
                }
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
                if (medico != null)
                {
                    query = query.Where(c => c.MedicoId == medico.Id);
                }
                else
                {
                    return View(new List<Consulta>());
                }
            }
            // Administrators see all consultations (no filter needed)

            var consultas = await query.OrderByDescending(c => c.DataHora).ToListAsync();
            return View(consultas);
        }

        // GET: Consultas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidade)
                .Include(c => c.Prontuario)
                .Include(c => c.Pagamentos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (consulta == null)
            {
                return NotFound();
            }

            return View(consulta);
        }

        // GET: Consultas/Create
        [Authorize(Roles = "Paciente,Administrador")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Paciente"))
            {
                var currentUserId = _userManager.GetUserId(User);
                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.UsuarioId == currentUserId);
                
                if (paciente == null)
                {
                    return RedirectToAction("Create", "Pacientes");
                }
                
                // Passar o ID do paciente para a view
                ViewBag.PacienteId = paciente.Id;
            }

            var consulta = new Consulta
            {
                DataHora = DateTime.Now.AddDays(1).Date.AddHours(9)
            };

            // Carregar lista de médicos
            ViewData["MedicoId"] = new SelectList(
                await _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidade)
                    .Select(m => new { 
                        m.Id, 
                        Nome = m.Usuario.Nome + " - " + m.Especialidade.Nome 
                    })
                    .ToListAsync(),
                "Id", "Nome");

            return View(consulta);
        }

        // POST: Consultas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Paciente,Administrador")]
        public async Task<IActionResult> Create([Bind("MedicoId,DataHora,DuracaoMinutos,Tipo,MotivoConsulta,ObservacoesGerais")] Consulta consulta, string Horario, int? pacienteId = null)
        {
            // Adicionar mais logs para depuração
            var formValues = string.Join(", ", Request.Form.Select(x => $"{x.Key}={x.Value}"));
            Console.WriteLine($"Form values: {formValues}");
            
            if (consulta == null)
            {
                TempData["ErrorMessage"] = "Dados do formulário não foram recebidos corretamente";
                return View(new Consulta());
            }

            // IMPORTANTE: Inicialize as propriedades de navegação para evitar erros de validação
            consulta.Medico = await _context.Medicos.FindAsync(consulta.MedicoId);
            
            var currentUserId = _userManager.GetUserId(User);
            
            // Verificar se temos data e horário e combiná-los
            if (!string.IsNullOrEmpty(Horario) && consulta.DataHora != default)
            {
                try {
                    // Separar os componentes de hora e minuto
                    var horaParts = Horario.Split(':');
                    if (horaParts.Length == 2 && 
                        int.TryParse(horaParts[0], out int hora) && 
                        int.TryParse(horaParts[1], out int minuto))
                    {
                        // Combinar a data com o horário selecionado
                        consulta.DataHora = consulta.DataHora.Date
                            .AddHours(hora)
                            .AddMinutes(minuto);
                        
                        Console.WriteLine($"Data e hora combinados: {consulta.DataHora}");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Formato de horário inválido.");
                        Console.WriteLine("Formato de horário inválido");
                    }
                }
                catch (Exception ex) {
                    ModelState.AddModelError("", $"Erro ao processar o horário: {ex.Message}");
                    Console.WriteLine($"Erro ao processar horário: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "É necessário selecionar uma data e um horário.");
                Console.WriteLine("Data ou horário não informados");
            }
            
            // Determinar o paciente
            if (User.IsInRole("Paciente"))
            {
                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.UsuarioId == currentUserId);
                
                if (paciente == null)
                {
                    return RedirectToAction("Create", "Pacientes");
                }
                
                consulta.PacienteId = paciente.Id;
                // IMPORTANTE: Inicialize a propriedade de navegação do paciente
                consulta.Paciente = paciente;
                Console.WriteLine($"Set PacienteId from user: {consulta.PacienteId}");
            }
            else if (User.IsInRole("Administrador") && pacienteId.HasValue)
            {
                consulta.PacienteId = pacienteId.Value;
                // IMPORTANTE: Inicialize a propriedade de navegação do paciente
                consulta.Paciente = await _context.Pacientes.FindAsync(pacienteId.Value);
                Console.WriteLine($"Set PacienteId from parameter: {consulta.PacienteId}");
            }
            else
            {
                ModelState.AddModelError("", "Paciente não identificado.");
                Console.WriteLine("Paciente não identificado");
            }

            // IMPORTANTE: Remova erros de validação para Medico e Paciente já que
            // estamos definindo essas propriedades manualmente
            ModelState.Remove("Medico");
            ModelState.Remove("Paciente");

            // Ensure we have a valid DateTime
            if (consulta.DataHora == default)
            {
                ModelState.AddModelError("DataHora", "A data e hora da consulta é obrigatória.");
            }
            
            // Validações de negócio
            if (consulta.DataHora <= DateTime.Now)
            {
                ModelState.AddModelError("DataHora", "A data e hora da consulta deve ser futura.");
            }

            // Verificar disponibilidade do médico
            var conflito = await _context.Consultas
                .AnyAsync(c => c.MedicoId == consulta.MedicoId && 
                             c.DataHora.Date == consulta.DataHora.Date &&
                             c.DataHora.Hour == consulta.DataHora.Hour &&
                             c.Status != StatusConsulta.Cancelada);

            if (conflito)
            {
                ModelState.AddModelError("DataHora", "Médico já possui consulta agendada neste horário.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    consulta.DataCriacao = DateTime.UtcNow;
                    consulta.Status = StatusConsulta.Agendada;
                    
                    _context.Add(consulta);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Consulta criada com sucesso, ID: {consulta.Id}");
                    
                    // Get patient and doctor information for the email
                    var paciente = await _context.Pacientes
                        .Include(p => p.Usuario)
                        .FirstOrDefaultAsync(p => p.Id == consulta.PacienteId);
                    
                    var medico = await _context.Medicos
                        .Include(m => m.Usuario)
                        .FirstOrDefaultAsync(m => m.Id == consulta.MedicoId);
                    
                    bool emailEnviado = false;
                    
                    if (paciente?.Usuario?.Email != null && medico != null)
                    {
                        // Extract time part from DateTime
                        string appointmentTime = consulta.DataHora.ToString("HH:mm");
                        
                        try 
                        {
                            // Send confirmation email
                            await _emailService.SendAppointmentConfirmationAsync(
                                paciente.Usuario.Email,
                                $"{paciente.Usuario.Nome} {paciente.Usuario.Sobrenome}",
                                $"{medico.Usuario.Nome} {medico.Usuario.Sobrenome}",
                                consulta.DataHora,
                                appointmentTime
                            );
                            emailEnviado = true;
                        }
                        catch (Exception emailEx)
                        {
                            // Log the error but don't fail the appointment creation
                            Console.WriteLine($"Erro ao enviar e-mail: {emailEx.Message}");
                        }
                    }

                    // Set success message including email status
                    if (emailEnviado)
                    {
                        TempData["Success"] = "Consulta agendada com sucesso! Um e-mail de confirmação foi enviado para você.";
                    }
                    else
                    {
                        TempData["Success"] = "Consulta agendada com sucesso! Não foi possível enviar o e-mail de confirmação.";
                    }

                    // Make sure we have a valid ID before redirecting
                    if (consulta.Id > 0)
                    {
                        // Explicitly redirect to the Details action with the ID
                        return RedirectToAction("Details", new { id = consulta.Id });
                    }
                    else
                    {
                        // If for some reason we don't have an ID, log and redirect to index
                        Console.WriteLine("Warning: Consulta ID not set after SaveChanges");
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error in Create action: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    ModelState.AddModelError("", $"Erro ao agendar consulta: {ex.Message}");
                    TempData["ErrorMessage"] = $"Erro ao agendar consulta: {ex.Message}";
                    TempData["DebugInfo"] = ex.ToString();
                }
            }
            else
            {
                // Debug model state errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                }
            }

            // Se chegarmos aqui, algo falhou, redisplay form
            ViewData["MedicoId"] = new SelectList(
                await _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidade)
                    .Select(m => new { 
                        m.Id, 
                        Nome = m.Usuario.Nome + " - " + m.Especialidade.Nome 
                    })
                    .ToListAsync(), 
                "Id", "Nome", consulta.MedicoId);

            return View(consulta);
        }

        // GET: Consultas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode editar
            bool canEdit = User.IsInRole("Administrador") ||
                          (User.IsInRole("Paciente") && consulta.Paciente.UsuarioId == currentUserId && consulta.Status == StatusConsulta.Agendada) ||
                          (User.IsInRole("Medico") && consulta.Medico.UsuarioId == currentUserId);

            if (!canEdit)
            {
                return Forbid();
            }

            ViewData["MedicoId"] = new SelectList(
                await _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidade)
                    .Select(m => new { 
                        m.Id, 
                        Nome = m.Usuario.Nome + " - " + m.Especialidade.Nome 
                    })
                    .ToListAsync(), 
                "Id", "Nome", consulta.MedicoId);

            return View(consulta);
        }

        // POST: Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PacienteId,MedicoId,DataHora,DuracaoMinutos,Status,Tipo,MotivoConsulta,ObservacoesGerais,Valor,Pago,DataCancelamento,MotivoCancelamento,DataCriacao")] Consulta consulta)
        {
            if (id != consulta.Id)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var consultaOriginal = await _context.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultaOriginal == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode editar
            bool canEdit = User.IsInRole("Administrador") ||
                          (User.IsInRole("Paciente") && consultaOriginal.Paciente.UsuarioId == currentUserId && consultaOriginal.Status == StatusConsulta.Agendada) ||
                          (User.IsInRole("Medico") && consultaOriginal.Medico.UsuarioId == currentUserId);

            if (!canEdit)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(consulta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultaExists(consulta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = consulta.Id });
            }

            ViewData["MedicoId"] = new SelectList(
                await _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidade)
                    .Select(m => new { 
                        m.Id, 
                        Nome = m.Usuario.Nome + " - " + m.Especialidade.Nome 
                    })
                    .ToListAsync(), 
                "Id", "Nome", consulta.MedicoId);

            return View(consulta);
        }

        // POST: Consultas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            var currentUserId = _userManager.GetUserId(User);
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode cancelar
            bool canCancel = User.IsInRole("Administrador") ||
                            consulta.Paciente.UsuarioId == currentUserId ||
                            consulta.Medico.UsuarioId == currentUserId;

            if (!canCancel)
            {
                return Forbid();
            }

            consulta.Status = StatusConsulta.Cancelada;
            consulta.DataCancelamento = DateTime.UtcNow;
            consulta.MotivoCancelamento = motivo ?? "Cancelamento solicitado pelo usuário";

            _context.Update(consulta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = consulta.Id });
        }

        // POST: Consultas/Confirmar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Medico,Administrador")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var consulta = await _context.Consultas
                .Include(c => c.Medico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
            {
                return NotFound();
            }

            // Verificar se o usuário pode confirmar
            bool canConfirm = User.IsInRole("Administrador") ||
                             consulta.Medico.UsuarioId == currentUserId;

            if (!canConfirm)
            {
                return Forbid();
            }

            consulta.Status = StatusConsulta.Confirmada;
            _context.Update(consulta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = consulta.Id });
        }

        private bool ConsultaExists(int id)
        {
            return _context.Consultas.Any(e => e.Id == id);
        }

        // GET: Consultas/Agendamento
        public async Task<IActionResult> Agendamento()
        {
            var especialidades = await _context.Especialidades.ToListAsync();
            ViewBag.Especialidades = especialidades;
            return View();
        }

        // GET: Consultas/MedicosDisponiveis
        public async Task<IActionResult> MedicosDisponiveis(int especialidadeId, DateTime data)
        {
            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .Where(m => m.EspecialidadeId == especialidadeId)
                .ToListAsync();

            ViewBag.DataSelecionada = data;
            ViewBag.EspecialidadeId = especialidadeId;
            
            return PartialView("_MedicosDisponiveis", medicos);
        }

        // POST: Consultas/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (consulta == null)
            {
                return NotFound();
            }

            try
            {
                // Update the status to canceled
                consulta.Status = StatusConsulta.Cancelada;
                
                _context.Update(consulta);
                await _context.SaveChangesAsync();

                // Send notification email
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendStatusChangeNotificationAsync(
                            consulta.Paciente.Usuario.Email,
                            $"{consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}",
                            $"{consulta.Medico.Usuario.Nome} {consulta.Medico.Usuario.Sobrenome}",
                            consulta.DataHora,
                            StatusConsulta.Cancelada
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the cancellation process
                        Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }

                TempData["Success"] = "Consulta cancelada com sucesso.";
                return RedirectToAction(nameof(Details), new { id = consulta.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao cancelar consulta: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = consulta.Id });
            }
        }
        
        // POST: Consultas/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, StatusConsulta newStatus)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (consulta == null)
            {
                return NotFound();
            }

            try
            {
                consulta.Status = newStatus;
                
                _context.Update(consulta);
                await _context.SaveChangesAsync();

                // Send status change notification email
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendStatusChangeNotificationAsync(
                            consulta.Paciente.Usuario.Email,
                            $"{consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}",
                            $"{consulta.Medico.Usuario.Nome} {consulta.Medico.Usuario.Sobrenome}",
                            consulta.DataHora,
                            newStatus
                        );
                        
                        // Also notify doctor if confirming or canceling
                        if ((newStatus == StatusConsulta.Confirmada || newStatus == StatusConsulta.Cancelada) && 
                            consulta.Medico?.Usuario?.Email != null)
                        {
                            await _emailService.SendEmailAsync(
                                consulta.Medico.Usuario.Email,
                                $"Consulta {(newStatus == StatusConsulta.Confirmada ? "confirmada" : "cancelada")}",
                                $@"
                                    <div style='font-family: Arial, sans-serif;'>
                                        <p>Olá Dr. {consulta.Medico.Usuario.Nome},</p>
                                        <p>Uma consulta foi {(newStatus == StatusConsulta.Confirmada ? "confirmada" : "cancelada")}:</p>
                                        <p>Paciente: {consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}<br>
                                        Data/Hora: {consulta.DataHora.ToString("dd/MM/yyyy HH:mm")}</p>
                                        <p>Por favor, verifique o sistema para mais detalhes.</p>
                                    </div>
                                "
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }
                
                string statusText = newStatus switch
                {
                    StatusConsulta.Confirmada => "confirmada",
                    StatusConsulta.Cancelada => "cancelada",
                    StatusConsulta.Concluida => "marcada como concluída",
                    _ => "atualizada"
                };

                TempData["Success"] = $"Consulta {statusText} com sucesso.";
                return RedirectToAction(nameof(Details), new { id = consulta.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao atualizar status da consulta: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = consulta.Id });
            }
        }
    }
}

