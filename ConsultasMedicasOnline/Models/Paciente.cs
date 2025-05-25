using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Paciente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TipoSanguineo { get; set; }

        [StringLength(500)]
        public string? Alergias { get; set; }

        [StringLength(500)]
        public string? MedicamentosEmUso { get; set; }

        [StringLength(1000)]
        public string? HistoricoFamiliar { get; set; }

        public string? ContatoEmergencia { get; set; }        public string? TelefoneEmergencia { get; set; }

        public DateTime DataCadastro { get; set; }

        // Relacionamentos
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
        public virtual ICollection<Prontuario> Prontuarios { get; set; } = new List<Prontuario>();
        public virtual Endereco? Endereco { get; set; }
    }
}
