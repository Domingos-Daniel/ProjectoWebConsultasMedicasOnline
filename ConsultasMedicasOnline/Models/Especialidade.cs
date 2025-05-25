using System.ComponentModel.DataAnnotations;

namespace ConsultasMedicasOnline.Models
{
    public class Especialidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public virtual ICollection<Medico> Medicos { get; set; } = new List<Medico>();
    }
}
