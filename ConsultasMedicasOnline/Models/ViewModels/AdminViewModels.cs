using ConsultasMedicasOnline.Models;

namespace ConsultasMedicasOnline.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalMedicos { get; set; }
        public int TotalPacientes { get; set; }
        public int TotalConsultas { get; set; }
        public int TotalEspecialidades { get; set; }
        public int ConsultasHoje { get; set; }
        public int ConsultasAgendadas { get; set; }
        public int ConsultasPendentesValidacao { get; set; }
        public List<Usuario> UltimosUsuarios { get; set; } = new();
        public List<Consulta> ConsultasRecentes { get; set; } = new();
    }

    public class CriarMedicoViewModel
    {
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Telefone { get; set; }
        public string CRM { get; set; }
        public int EspecialidadeId { get; set; }
        public decimal ValorConsulta { get; set; }
    }

    public class RelatoriosViewModel
    {
        public dynamic ConsultasPorMes { get; set; }
        public dynamic ConsultasPorEspecialidade { get; set; }
    }

    public class UsuarioComRoleViewModel
    {
        public Usuario Usuario { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class EditarMedicoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string CRM { get; set; }
        public string EstadoCRM { get; set; }
        public int EspecialidadeId { get; set; }
        public decimal ValorConsulta { get; set; }
        public string? Biografia { get; set; }
        public bool AceitaParticular { get; set; }
        public bool AceitaConvenio { get; set; }
    }

    public class LogViewModel
    {
        public DateTime Data { get; set; }
        public string Acao { get; set; }
        public string Usuario { get; set; }
    }
}
