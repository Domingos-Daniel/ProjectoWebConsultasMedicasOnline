using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Paciente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O ID do usuário é obrigatório")]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número de identificação é obrigatório")]
        [Display(Name = "Número de Identificação")]
        [StringLength(20, ErrorMessage = "O número de identificação deve ter no máximo 20 caracteres")]
        public string NumeroIdentificacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Display(Name = "Telefone")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        public string Telefone { get; set; } = string.Empty;

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [Display(Name = "Gênero")]
        [StringLength(10)]
        public string? Genero { get; set; }

        [Display(Name = "Tipo Sanguíneo")]
        [StringLength(10)]
        public string? TipoSanguineo { get; set; }

        [Display(Name = "Alergias")]
        [StringLength(1000)]
        public string? Alergias { get; set; }

        [Display(Name = "Medicamentos em Uso")]
        [StringLength(1000)]
        public string? MedicamentosEmUso { get; set; }

        [Display(Name = "Histórico Familiar")]
        [StringLength(2000)]
        public string? HistoricoFamiliar { get; set; }

        [Display(Name = "Contato de Emergência")]
        [StringLength(100)]
        public string? ContatoEmergencia { get; set; }

        [Display(Name = "Telefone de Emergência")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [StringLength(20)]
        public string? TelefoneEmergencia { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        // Relacionamentos
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
        public virtual ICollection<Prontuario> Prontuarios { get; set; } = new List<Prontuario>();
        public virtual Endereco? Endereco { get; set; }
    }
}
