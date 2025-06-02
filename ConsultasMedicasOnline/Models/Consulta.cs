using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public enum StatusConsulta
    {
        Agendada = 1,
        Confirmada = 2,
        EmAndamento = 3,
        Concluida = 4,
        Cancelada = 5,
        NaoCompareceu = 6
    }

    public enum TipoConsulta
    {
        Presencial = 1,
        Online = 2
    }

    public class Consulta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }
        
        [Required]
        public int MedicoId { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        public int DuracaoMinutos { get; set; } = 30;

        [Required]
        [Display(Name = "Status da Consulta")]
        public StatusConsulta Status { get; set; } = StatusConsulta.Agendada;
        public TipoConsulta Tipo { get; set; } = TipoConsulta.Presencial;

        [StringLength(500)]
        public string? MotivoConsulta { get; set; }

        [StringLength(1000)]
        public string? ObservacoesGerais { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        public bool Pago { get; set; } = false;        public DateTime? DataCancelamento { get; set; }
        public string? MotivoCancelamento { get; set; }

        public DateTime DataCriacao { get; set; }

        // Relacionamentos
        [ForeignKey("PacienteId")]
        public virtual Paciente Paciente { get; set; } = null!;

        [ForeignKey("MedicoId")]
        public virtual Medico Medico { get; set; } = null!;

        public virtual Prontuario? Prontuario { get; set; }
        public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}
