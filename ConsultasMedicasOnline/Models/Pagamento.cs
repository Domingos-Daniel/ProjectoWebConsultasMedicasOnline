using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsultasMedicasOnline.Models
{
    public enum StatusPagamento
    {
        Pendente = 1,
        Processando = 2,
        Aprovado = 3,
        Recusado = 4,
        Estornado = 5
    }

    public enum TipoPagamento
    {
        CartaoCredito = 1,
        CartaoDebito = 2,
        PIX = 3,
        Boleto = 4,
        Dinheiro = 5
    }

    public class Pagamento
    {
        [Key]
        public int Id { get; set; }

        public int ConsultaId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        public TipoPagamento TipoPagamento { get; set; }
        public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

        [StringLength(100)]
        public string? TransacaoId { get; set; }

        [StringLength(500)]
        public string? ObservacoesPagamento { get; set; }

        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
        public DateTime? DataProcessamento { get; set; }

        // Relacionamentos
        [ForeignKey("ConsultaId")]
        public virtual Consulta Consulta { get; set; } = null!;

        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
