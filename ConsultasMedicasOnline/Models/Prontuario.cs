using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Prontuario
    {
        [Key]
        public int Id { get; set; }

        public int ConsultaId { get; set; }
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }

        [StringLength(1000)]
        public string? HistoriaClinica { get; set; }

        [StringLength(500)]
        public string? ExameFisico { get; set; }

        [StringLength(500)]
        public string? Hipoteses { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnostico { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Tratamento { get; set; }

        [StringLength(1000)]
        public string? Prescricoes { get; set; }

        [StringLength(500)]
        public string? ExamesSolicitados { get; set; }

        [StringLength(500)]
        public string? OrientacoesGerais { get; set; }

        public DateTime? ProximaConsulta { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("ConsultaId")]
        public virtual Consulta Consulta { get; set; } = null!;

        [ForeignKey("PacienteId")]
        public virtual Paciente Paciente { get; set; } = null!;

        [ForeignKey("MedicoId")]
        public virtual Medico Medico { get; set; } = null!;
    }
}
