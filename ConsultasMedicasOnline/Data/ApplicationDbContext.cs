using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConsultasMedicasOnline.Models;
using System.Threading.Tasks;

namespace ConsultasMedicasOnline.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets para as entidades do sistema
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Especialidade> Especialidades { get; set; }
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<Prontuario> Prontuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);        // Configuração de relacionamentos
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Paciente)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Paciente>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Medico)
            .WithOne(m => m.Usuario)
            .HasForeignKey<Medico>(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurações específicas para Consulta - evitar múltiplos relacionamentos com Usuario
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Paciente)
            .WithMany(p => p.Consultas)
            .HasForeignKey(c => c.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Medico)
            .WithMany(m => m.Consultas)
            .HasForeignKey(c => c.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento um-para-um entre Consulta e Prontuario
        modelBuilder.Entity<Prontuario>()
            .HasOne(p => p.Consulta)
            .WithOne(c => c.Prontuario)
            .HasForeignKey<Prontuario>(p => p.ConsultaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento um-para-um entre Paciente e Endereco
        modelBuilder.Entity<Endereco>()
            .HasOne(e => e.Paciente)
            .WithOne(p => p.Endereco)
            .HasForeignKey<Endereco>(e => e.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento entre Medico e HorarioDisponivel
        modelBuilder.Entity<HorarioDisponivel>()
            .HasOne(h => h.Medico)
            .WithMany(m => m.HorariosDisponiveis)
            .HasForeignKey(h => h.MedicoId)
            .OnDelete(DeleteBehavior.Cascade);        // Relacionamento entre Consulta e Pagamento
        modelBuilder.Entity<Pagamento>()
            .HasOne(p => p.Consulta)
            .WithMany(c => c.Pagamentos)
            .HasForeignKey(p => p.ConsultaId)
            .OnDelete(DeleteBehavior.Cascade);        // Relacionamento entre Medico e Especialidade
        modelBuilder.Entity<Medico>()
            .HasOne(m => m.Especialidade)
            .WithMany(e => e.Medicos)
            .HasForeignKey(m => m.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento entre Pagamento e Transacao
        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Pagamento)
            .WithMany(p => p.Transacoes)
            .HasForeignKey(t => t.PagamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuração de índices únicos
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.CPF)
            .IsUnique()
            .HasFilter("[CPF] IS NOT NULL");

        modelBuilder.Entity<Medico>()
            .HasIndex(m => new { m.CRM, m.EstadoCRM })
            .IsUnique();        // Seed de dados iniciais
        modelBuilder.Entity<Especialidade>().HasData(
            new Especialidade { Id = 1, Nome = "Cardiologia", Descricao = "Especialidade médica que se ocupa do diagnóstico e tratamento das doenças que acometem o coração", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 2, Nome = "Dermatologia", Descricao = "Especialidade médica que se ocupa do diagnóstico e tratamento das doenças da pele", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 3, Nome = "Pediatria", Descricao = "Especialidade médica dedicada à assistência à criança e ao adolescente", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 4, Nome = "Ginecologia", Descricao = "Especialidade médica que trata das doenças do sistema reprodutor feminino", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 5, Nome = "Ortopedia", Descricao = "Especialidade médica que cuida do aparelho locomotor", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 6, Nome = "Neurologia", Descricao = "Especialidade médica que trata dos distúrbios do sistema nervoso", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 7, Nome = "Psiquiatria", Descricao = "Especialidade médica que se dedica ao estudo, diagnóstico e tratamento dos transtornos mentais", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 8, Nome = "Oftalmologia", Descricao = "Especialidade médica que investiga e trata as doenças relacionadas aos olhos", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 9, Nome = "Otorrinolaringologia", Descricao = "Especialidade médica que cuida do ouvido, nariz e garganta", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) },
            new Especialidade { Id = 10, Nome = "Clínica Geral", Descricao = "Atendimento médico geral e preventivo", Ativa = true, DataCriacao = new DateTime(2025, 1, 1) }
        );
    }

    // Método para semear dados iniciais
    public static async Task SeedData(ApplicationDbContext context, 
                                     UserManager<Usuario> userManager, 
                                     RoleManager<IdentityRole> roleManager)
    {
        // Garantir que os papéis existem
        if (!await roleManager.RoleExistsAsync("Administrador"))
            await roleManager.CreateAsync(new IdentityRole("Administrador"));
        
        if (!await roleManager.RoleExistsAsync("Medico"))
            await roleManager.CreateAsync(new IdentityRole("Medico"));
        
        if (!await roleManager.RoleExistsAsync("Paciente"))
            await roleManager.CreateAsync(new IdentityRole("Paciente"));
        
        // Semear médicos
        await Seeders.MedicoSeeder.SeedMedicos(context, userManager);
    }
}
