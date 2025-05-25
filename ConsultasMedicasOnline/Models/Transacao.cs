using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public class Transacao
    {
        [Key]
        public int Id { get; set; }

        public int PagamentoId { get; set; }

        [Required]
        [StringLength(100)]
        public string TransacaoExternaId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? DetalhesResposta { get; set; }

        public DateTime DataTransacao { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("PagamentoId")]
        public virtual Pagamento Pagamento { get; set; } = null!;
    }
}
