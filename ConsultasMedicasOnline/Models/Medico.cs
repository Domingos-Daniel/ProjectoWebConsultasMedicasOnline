using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Medico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CRM { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string EstadoCRM { get; set; } = string.Empty;

        public int EspecialidadeId { get; set; }

        [StringLength(1000)]
        public string? Biografia { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorConsulta { get; set; }

        public int TempoConsultaMinutos { get; set; } = 30;        public bool AceitaParticular { get; set; } = true;
        public bool AceitaConvenio { get; set; } = false;

        public DateTime DataCadastro { get; set; }

        // Relacionamentos
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("EspecialidadeId")]
        public virtual Especialidade Especialidade { get; set; } = null!;

        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
        public virtual ICollection<HorarioDisponivel> HorariosDisponiveis { get; set; } = new List<HorarioDisponivel>();
        public virtual ICollection<Prontuario> Prontuarios { get; set; } = new List<Prontuario>();
    }
}
