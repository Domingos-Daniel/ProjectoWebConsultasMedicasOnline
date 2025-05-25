using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ConsultasMedicasOnline.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Sobrenome { get; set; } = string.Empty;

        [StringLength(11)]
        public string? CPF { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [StringLength(15)]
        public string? Telefone { get; set; }

        public string NomeCompleto => $"{Nome} {Sobrenome}";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public bool Ativo { get; set; } = true;        // Relacionamentos
        public virtual Medico? Medico { get; set; }
        public virtual Paciente? Paciente { get; set; }
    }
}
