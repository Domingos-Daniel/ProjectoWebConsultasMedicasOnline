using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using ConsultasMedicasOnline.Models.ViewModels;
using ConsultasMedicasOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultasMedicasOnline.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeedDemoData()
        {
            await ApplicationDbContext.SeedData(_context, _userManager, _roleManager);
            TempData["Message"] = "Dados de demonstração foram adicionados com sucesso!";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsuarios = await _userManager.Users.CountAsync(),
                TotalMedicos = await _context.Medicos.CountAsync(),
                TotalPacientes = await _context.Pacientes.CountAsync(),
                TotalConsultas = await _context.Consultas.CountAsync(),
                TotalEspecialidades = await _context.Especialidades.CountAsync(),
                ConsultasHoje = await _context.Consultas.CountAsync(c => c.DataHora.Date == DateTime.Today),
                ConsultasAgendadas = await _context.Consultas.CountAsync(c => c.Status == StatusConsulta.Agendada),
                ConsultasPendentesValidacao = await _context.Consultas.CountAsync(c => c.Status == StatusConsulta.Agendada),
                UltimosUsuarios = await _userManager.Users.OrderByDescending(u => u.DataCriacao).Take(5).ToListAsync(),
                ConsultasRecentes = await _context.Consultas
                    .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                    .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                    .OrderByDescending(c => c.DataCriacao)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        // Gestão de Médicos
        public async Task<IActionResult> Medicos()
        {
            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .ToListAsync();
            return View(medicos);
        }

        [HttpGet]
        public async Task<IActionResult> CriarMedico()
        {
            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CriarMedico(CriarMedicoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    DataCriacao = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(usuario, model.Senha);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, "Medico");

                    var medico = new Medico
                    {
                        UsuarioId = usuario.Id,
                        CRM = model.CRM,
                        EspecialidadeId = model.EspecialidadeId,
                        ValorConsulta = model.ValorConsulta
                    };

                    _context.Medicos.Add(medico);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Médico criado com sucesso!";
                    return RedirectToAction(nameof(Medicos));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();
            return View(model);
        }

        // Gestão de Pacientes
        public async Task<IActionResult> Pacientes()
        {
            var pacientes = await _context.Pacientes
                .Include(p => p.Usuario)
                .ToListAsync();
            return View(pacientes);
        }

        // Gestão de Especialidades
        public async Task<IActionResult> Especialidades()
        {
            var especialidades = await _context.Especialidades.ToListAsync();
            return View(especialidades);
        }

        [HttpPost]
        public async Task<IActionResult> CriarEspecialidade(string nome, string descricao)
        {
            if (!string.IsNullOrEmpty(nome))
            {
                var especialidade = new Especialidade
                {
                    Nome = nome,
                    Descricao = descricao
                };

                _context.Especialidades.Add(especialidade);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Especialidade criada com sucesso!";
            }

            return RedirectToAction(nameof(Especialidades));
        }

        // Validação de Agendamentos
        public async Task<IActionResult> ValidarAgendamentos()
        {
            var consultasPendentes = await _context.Consultas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Especialidade)
                .Where(c => c.Status == StatusConsulta.Agendada)
                .OrderBy(c => c.DataHora)
                .ToListAsync();

            return View(consultasPendentes);
        }

        [HttpPost]
        public async Task<IActionResult> AprovarConsulta(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
            {
                TempData["Error"] = "Consulta não encontrada.";
                return RedirectToAction(nameof(ValidarAgendamentos));
            }

            try
            {
                // Atualizar status da consulta
                consulta.Status = StatusConsulta.Confirmada;
                _context.Update(consulta);
                await _context.SaveChangesAsync();

                // Enviar email ao paciente
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendStatusChangeNotificationAsync(
                            consulta.Paciente.Usuario.Email,
                            $"{consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}",
                            $"{consulta.Medico.Usuario.Nome} {consulta.Medico.Usuario.Sobrenome}",
                            consulta.DataHora,
                            StatusConsulta.Confirmada
                        );
                    }
                    catch (Exception ex)
                    {
                        // Registrar erro, mas não falhar o processo
                        Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }

                // Enviar email ao médico
                if (consulta.Medico?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Medico.Usuario.Email,
                            "Nova consulta confirmada",
                            $@"
                                <div style='font-family: Arial, sans-serif;'>
                                    <h2>Nova consulta aprovada</h2>
                                    <p>Olá Dr. {consulta.Medico.Usuario.Nome},</p>
                                    <p>Uma nova consulta foi confirmada em sua agenda:</p>
                                    <p>
                                        <strong>Paciente:</strong> {consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}<br>
                                        <strong>Data/Hora:</strong> {consulta.DataHora:dd/MM/yyyy HH:mm}<br>
                                        <strong>Tipo:</strong> {(consulta.Tipo == TipoConsulta.Presencial ? "Presencial" : "Online")}
                                    </p>
                                    <p>Por favor, acesse o sistema para mais detalhes.</p>
                                </div>
                            "
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail para o médico: {ex.Message}");
                    }
                }

                TempData["Success"] = $"Consulta aprovada com sucesso. E-mails enviados para {consulta.Paciente.Usuario.Nome} e Dr. {consulta.Medico.Usuario.Nome}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao aprovar consulta: {ex.Message}";
            }

            return RedirectToAction(nameof(ValidarAgendamentos));
        }

        [HttpPost]
        public async Task<IActionResult> RejeitarConsulta(int id, string motivo)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
            {
                TempData["Error"] = "Consulta não encontrada.";
                return RedirectToAction(nameof(ValidarAgendamentos));
            }

            try
            {
                // Atualizar status da consulta
                consulta.Status = StatusConsulta.Cancelada;
                consulta.ObservacoesGerais = $"Consulta rejeitada pelo administrador. Motivo: {motivo}\n\n" +
                                            (consulta.ObservacoesGerais ?? "");
                _context.Update(consulta);
                await _context.SaveChangesAsync();

                // Enviar email ao paciente
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Paciente.Usuario.Email,
                            "Consulta não aprovada",
                            $@"
                                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                                    <div style='text-align: center; padding: 10px; background-color: #fef2f2; border-radius: 5px;'>
                                        <h2 style='color: #b91c1c;'>Consulta Não Aprovada</h2>
                                    </div>

                                    <div style='padding: 20px;'>
                                        <p>Olá, <strong>{consulta.Paciente.Usuario.Nome}</strong>!</p>

                                        <p>Infelizmente, sua consulta com <strong>Dr. {consulta.Medico.Usuario.Nome}</strong> não pôde ser aprovada.</p>

                                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                            <p><strong>Data/Hora solicitada:</strong> {consulta.DataHora:dd/MM/yyyy HH:mm}</p>
                                            <p><strong>Motivo:</strong> {motivo}</p>
                                        </div>

                                        <p>Por favor, tente agendar uma nova consulta em um horário diferente ou entre em contato com nosso atendimento.</p>
                                    </div>

                                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                                        <p style='color: #64748b; font-size: 14px;'>Este é um e-mail automático. Por favor, não responda diretamente.</p>
                                    </div>
                                </div>
                            "
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }

                // Enviar email ao médico (opcional)
                if (consulta.Medico?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Medico.Usuario.Email,
                            "Consulta rejeitada",
                            $@"
                                <div style='font-family: Arial, sans-serif;'>
                                    <h2>Consulta rejeitada</h2>
                                    <p>Olá Dr. {consulta.Medico.Usuario.Nome},</p>
                                    <p>Uma solicitação de consulta foi rejeitada:</p>
                                    <p>
                                        <strong>Paciente:</strong> {consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}<br>
                                        <strong>Data/Hora:</strong> {consulta.DataHora:dd/MM/yyyy HH:mm}<br>
                                        <strong>Motivo:</strong> {motivo}
                                    </p>
                                </div>
                            "
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail para o médico: {ex.Message}");
                    }
                }

                TempData["Success"] = $"Consulta rejeitada com sucesso. E-mails de notificação enviados.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao rejeitar consulta: {ex.Message}";
            }

            return RedirectToAction(nameof(ValidarAgendamentos));
        }

        // Relatórios
        public async Task<IActionResult> Relatorios()
        {
            var relatorios = new RelatoriosViewModel
            {
                ConsultasPorMes = await _context.Consultas
                    .GroupBy(c => new { c.DataHora.Year, c.DataHora.Month })
                    .Select(g => new { Periodo = $"{g.Key.Month}/{g.Key.Year}", Total = g.Count() })
                    .ToListAsync(),

                ConsultasPorEspecialidade = await _context.Consultas
                    .Include(c => c.Medico).ThenInclude(m => m.Especialidade)
                    .GroupBy(c => c.Medico.Especialidade.Nome)
                    .Select(g => new { Especialidade = g.Key, Total = g.Count() })
                    .ToListAsync()
            };

            return View(relatorios);
        }

        // Gestão de Usuários
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _userManager.Users
                .OrderByDescending(u => u.DataCriacao)
                .ToListAsync();

            var usuariosComRoles = new List<UsuarioComRoleViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                usuariosComRoles.Add(new UsuarioComRoleViewModel
                {
                    Usuario = usuario,
                    Roles = roles.ToList()
                });
            }

            return View(usuariosComRoles);
        }

        [HttpPost]
        public async Task<IActionResult> BloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                usuario.Ativo = false;
                await _userManager.UpdateAsync(usuario);
                TempData["Success"] = "Usuário bloqueado com sucesso!";
            }
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        public async Task<IActionResult> DesbloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                usuario.Ativo = true;
                await _userManager.UpdateAsync(usuario);
                TempData["Success"] = "Usuário desbloqueado com sucesso!";
            }
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpGet]
        public async Task<IActionResult> EditarMedico(int id)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidade)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null)
                return NotFound();

            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();

            var model = new EditarMedicoViewModel
            {
                Id = medico.Id,
                Nome = medico.Usuario.Nome,
                Sobrenome = medico.Usuario.Sobrenome,
                Email = medico.Usuario.Email,
                Telefone = medico.Usuario.PhoneNumber,
                CRM = medico.CRM,
                EstadoCRM = medico.EstadoCRM,
                EspecialidadeId = medico.EspecialidadeId,
                ValorConsulta = medico.ValorConsulta ?? 0,
                Biografia = medico.Biografia,
                AceitaParticular = medico.AceitaParticular,
                AceitaConvenio = medico.AceitaConvenio
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditarMedico(EditarMedicoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var medico = await _context.Medicos
                    .Include(m => m.Usuario)
                    .FirstOrDefaultAsync(m => m.Id == model.Id);

                if (medico != null)
                {
                    // Atualizar dados do usuário
                    medico.Usuario.Nome = model.Nome;
                    medico.Usuario.Sobrenome = model.Sobrenome;
                    medico.Usuario.PhoneNumber = model.Telefone;

                    // Atualizar dados do médico
                    medico.CRM = model.CRM;
                    medico.EstadoCRM = model.EstadoCRM;
                    medico.EspecialidadeId = model.EspecialidadeId;
                    medico.ValorConsulta = model.ValorConsulta;
                    medico.Biografia = model.Biografia;
                    medico.AceitaParticular = model.AceitaParticular;
                    medico.AceitaConvenio = model.AceitaConvenio;

                    await _userManager.UpdateAsync(medico.Usuario);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Médico atualizado com sucesso!";
                    return RedirectToAction(nameof(Medicos));
                }
            }

            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirMedico(int id)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico != null)
            {
                // Verificar se o médico tem consultas
                var temConsultas = await _context.Consultas.AnyAsync(c => c.MedicoId == id);

                if (temConsultas)
                {
                    TempData["Error"] = "Não é possível excluir este médico pois ele possui consultas cadastradas.";
                }
                else
                {
                    _context.Medicos.Remove(medico);
                    await _userManager.DeleteAsync(medico.Usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Médico excluído com sucesso!";
                }
            }

            return RedirectToAction(nameof(Medicos));
        }

        [HttpPost]
        public async Task<IActionResult> EditarEspecialidade(int id, string nome, string descricao, bool ativa)
        {
            var especialidade = await _context.Especialidades.FindAsync(id);
            if (especialidade != null)
            {
                especialidade.Nome = nome;
                especialidade.Descricao = descricao;
                especialidade.Ativa = ativa;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Especialidade atualizada com sucesso!";
            }

            return RedirectToAction(nameof(Especialidades));
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirEspecialidade(int id)
        {
            var especialidade = await _context.Especialidades.FindAsync(id);
            if (especialidade != null)
            {
                var temMedicos = await _context.Medicos.AnyAsync(m => m.EspecialidadeId == id);

                if (temMedicos)
                {
                    TempData["Error"] = "Não é possível excluir esta especialidade pois existem médicos cadastrados nela.";
                }
                else
                {
                    _context.Especialidades.Remove(especialidade);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Especialidade excluída com sucesso!";
                }
            }

            return RedirectToAction(nameof(Especialidades));
        }

        // Gestão de Consultas
        // GET: Admin/TodasConsultas
        public async Task<IActionResult> TodasConsultas()
        {
            var consultas = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidade)
                .OrderByDescending(c => c.DataHora)
                .ToListAsync();
                
            return View(consultas);
        }

        // POST: Admin/RejeitarConsulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarConsulta(int id, string motivo, string returnUrl = null)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (consulta == null)
            {
                TempData["Error"] = "Consulta não encontrada.";
                return RedirectToAction(returnUrl ?? nameof(ValidarAgendamentos));
            }
            
            try
            {
                // Atualizar status da consulta
                consulta.Status = StatusConsulta.Cancelada;
                consulta.ObservacoesGerais = $"Consulta rejeitada pelo administrador. Motivo: {motivo}\n\n" + 
                                            (consulta.ObservacoesGerais ?? "");
                _context.Update(consulta);
                await _context.SaveChangesAsync();
                
                // Enviar email ao paciente
                if (consulta.Paciente?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Paciente.Usuario.Email,
                            "Consulta não aprovada",
                            $@"
                                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                                    <div style='text-align: center; padding: 10px; background-color: #fef2f2; border-radius: 5px;'>
                                        <h2 style='color: #b91c1c;'>Consulta Não Aprovada</h2>
                                    </div>

                                    <div style='padding: 20px;'>
                                        <p>Olá, <strong>{consulta.Paciente.Usuario.Nome}</strong>!</p>

                                        <p>Infelizmente, sua consulta com <strong>Dr. {consulta.Medico.Usuario.Nome}</strong> não pôde ser aprovada.</p>

                                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                            <p><strong>Data/Hora solicitada:</strong> {consulta.DataHora:dd/MM/yyyy HH:mm}</p>
                                            <p><strong>Motivo:</strong> {motivo}</p>
                                        </div>

                                        <p>Por favor, tente agendar uma nova consulta em um horário diferente ou entre em contato com nosso atendimento.</p>
                                    </div>

                                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                                        <p style='color: #64748b; font-size: 14px;'>Este é um e-mail automático. Por favor, não responda diretamente.</p>
                                    </div>
                                </div>
                            "
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }
                
                // Enviar email ao médico (opcional)
                if (consulta.Medico?.Usuario?.Email != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            consulta.Medico.Usuario.Email,
                            "Consulta rejeitada",
                            $@"
                                <div style='font-family: Arial, sans-serif;'>
                                    <h2>Consulta rejeitada</h2>
                                    <p>Olá Dr. {consulta.Medico.Usuario.Nome},</p>
                                    <p>Uma solicitação de consulta foi rejeitada:</p>
                                    <p>
                                        <strong>Paciente:</strong> {consulta.Paciente.Usuario.Nome} {consulta.Paciente.Usuario.Sobrenome}<br>
                                        <strong>Data/Hora:</strong> {consulta.DataHora:dd/MM/yyyy HH:mm}<br>
                                        <strong>Motivo:</strong> {motivo}
                                    </p>
                                </div>
                            "
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail para o médico: {ex.Message}");
                    }
                }
                
                TempData["Success"] = $"Consulta rejeitada com sucesso. E-mails de notificação enviados.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao rejeitar consulta: {ex.Message}";
            }
            
            return RedirectToAction(returnUrl ?? nameof(ValidarAgendamentos));
        }

        // Configurações do Sistema
        public IActionResult Configuracoes()
        {
            return View();
        }

        // Logs do Sistema
        public async Task<IActionResult> Logs()
        {
            // Implementar sistema de logs se necessário
            var logs = new List<LogViewModel>
            {
                new LogViewModel { Data = DateTime.Now, Acao = "Sistema iniciado", Usuario = "Sistema" },
                new LogViewModel { Data = DateTime.Now.AddMinutes(-30), Acao = "Usuário admin logou", Usuario = "admin@medconsulta.ao" }
            };

            return View(logs);
        }
    }
}
