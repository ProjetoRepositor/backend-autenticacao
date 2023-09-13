namespace App.DB;

using App.Models;
using Microsoft.EntityFrameworkCore;

public class Contexto : DbContext
{
    // Get username and password from environment variable
    private static readonly string DbUsuario = 
        Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
    private static readonly string DbSenha = 
        Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "!Senha@Segura#123";
    private static readonly string DbUrl =
        Environment.GetEnvironmentVariable("POSTGRES_URL") ?? "localhost";
    private static readonly string DbNome =
        Environment.GetEnvironmentVariable("DB_NAME") ?? "repositor";

    private static readonly string ConnectionString = $"Host={DbUrl}; Database={DbNome}; Username={DbUsuario}; Password={DbSenha};";

    public DbSet<Usuario> usuario { get; set; }
    public DbSet<Sexo> sexo { get; set; }
    public DbSet<Login> login { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString, p => {
            p.EnableRetryOnFailure();
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Usuario>().Property(b => b.Nome).HasColumnName("nome").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.CPF).HasColumnName("cpf").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.DataNascimento).HasColumnName("datanascimento").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.IdSexo).HasColumnName("fk_idsexo").IsRequired();

        modelBuilder.Entity<Sexo>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Sexo>().Property(b => b.Descricao).HasColumnName("descricao").IsRequired();

        modelBuilder.Entity<Login>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Login>().Property(b => b.Email).HasColumnName("email").IsRequired();
        modelBuilder.Entity<Login>().Property(b => b.Senha).HasColumnName("senha").IsRequired();
        modelBuilder.Entity<Login>().Property(b => b.CodigoAutenticacao).HasColumnName("codigoautenticacao").IsRequired();
        modelBuilder.Entity<Login>().Property(b => b.IdUsuario).HasColumnName("fk_idusuario").IsRequired();
        
        // Definindo Relacionamentos
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Sexo) 
            .WithMany(s => s.Usuarios)
            .IsRequired();
        
        modelBuilder.Entity<Login>()
            .HasOne(l => l.Usuario) // Estabelece o relacionamento com a entidade Usuario
            .WithOne(u => u.Login) // Define a propriedade de navegação inversa em Usuario (se necessário)
            .HasForeignKey<Login>(l => l.IdUsuario) // Define a chave estrangeira na entidade Login
            .IsRequired();
    }

}