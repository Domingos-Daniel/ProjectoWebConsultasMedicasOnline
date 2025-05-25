using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public enum DiaSemana
    {
        Segunda = 1,
        Terca = 2,
        Quarta = 3,
        Quinta = 4,
        Sexta = 5,
        Sabado = 6,
        Domingo = 7
    }

    public class HorarioDisponivel
    {
        [Key]
        public int Id { get; set; }

        public int MedicoId { get; set; }

        public DiaSemana DiaSemana { get; set; }

        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFim { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("MedicoId")]
        public virtual Medico Medico { get; set; } = null!;
    }
}
