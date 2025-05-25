using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Endereco
    {
        [Key]
        public int Id { get; set; }

        public int PacienteId { get; set; }

        [Required]
        [StringLength(9)]
        public string CEP { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Logradouro { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Numero { get; set; }

        [StringLength(100)]
        public string? Complemento { get; set; }

        [Required]
        [StringLength(100)]
        public string Bairro { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Estado { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("PacienteId")]
        public virtual Paciente Paciente { get; set; } = null!;
    }
}
