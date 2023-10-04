namespace App.DB;

using Models;
using Microsoft.EntityFrameworkCore;

public class Contexto : DbContext
{
    #region Variáveis do banco de dados
    
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
    
    #endregion

    #region DbSets

    public DbSet<Usuario> Usuario { get; set; } = null!;
    public DbSet<Sexo> Sexo { get; set; } = null!;
    public DbSet<Login> Login { get; set; } = null!;
    public DbSet<Sessao> Sessao { get; set; } = null!;
    public DbSet<Conta> Conta { get; set; } = null!;
    
    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString, p => {
            p.EnableRetryOnFailure();
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Mapa de Tabelas

        modelBuilder.Entity<Usuario>().ToTable("usuario");
        modelBuilder.Entity<Login>().ToTable("login");
        modelBuilder.Entity<Sexo>().ToTable("sexo");
        modelBuilder.Entity<Sessao>().ToTable("sessao");
        modelBuilder.Entity<Conta>().ToView("conta");

        #endregion
        
        #region Mapa de Propriedades e Coludas
        
        modelBuilder.Entity<Usuario>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Usuario>().Property(b => b.Nome).HasColumnName("nome").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.Cpf).HasColumnName("cpf").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.DataNascimento).HasColumnName("datanascimento").IsRequired();
        modelBuilder.Entity<Usuario>().Property(b => b.IdSexo).HasColumnName("fk_idsexo").IsRequired();

        modelBuilder.Entity<Sexo>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Sexo>().Property(b => b.Descricao).HasColumnName("descricao").IsRequired();

        modelBuilder.Entity<Login>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Login>().Property(b => b.Email).HasColumnName("email").IsRequired();
        modelBuilder.Entity<Login>().Property(b => b.Senha).HasColumnName("senha").IsRequired();
        modelBuilder.Entity<Login>().Property(b => b.CodigoAutenticacao).HasColumnName("codigoautenticacao");
        modelBuilder.Entity<Login>().Property(b => b.Ativo).HasColumnName("ativo");
        modelBuilder.Entity<Login>().Property(b => b.IdUsuario).HasColumnName("fk_idusuario").IsRequired();

        modelBuilder.Entity<Sessao>().Property(s => s.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Sessao>().Property(s => s.HashSessao).HasColumnName("hashsessao").IsRequired();
        modelBuilder.Entity<Sessao>().Property(s => s.UltimoAcesso).HasColumnName("ultimoacesso").IsRequired();
        modelBuilder.Entity<Sessao>().Property(s => s.ManterLogin).HasColumnName("manterlogin").IsRequired();
        modelBuilder.Entity<Sessao>().Property(s => s.IdUsuario).HasColumnName("fk_idusuario").IsRequired();

        modelBuilder.Entity<Conta>().Property(c => c.Id).HasColumnName("id");
        modelBuilder.Entity<Conta>().Property(c => c.Nome).HasColumnName("nome");
        modelBuilder.Entity<Conta>().Property(c => c.Cpf).HasColumnName("cpf");
        modelBuilder.Entity<Conta>().Property(c => c.DataNascimento).HasColumnName("datanascimento");
        modelBuilder.Entity<Conta>().Property(c => c.Email).HasColumnName("email");
        modelBuilder.Entity<Conta>().Property(c => c.Sexo).HasColumnName("sexo");
        modelBuilder.Entity<Conta>().Property(c => c.Ativo).HasColumnName("ativo");
        
        #endregion
        
        #region Relacionamentos
        
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Sexo) 
            .WithMany(s => s.Usuarios)
            .HasForeignKey(u => u.IdSexo)
            .IsRequired();
        
        modelBuilder.Entity<Login>()
            .HasOne(l => l.Usuario)
            .WithOne(u => u.Login)
            .HasForeignKey<Login>(l => l.IdUsuario)
            .IsRequired();

        modelBuilder.Entity<Sessao>()
            .HasOne(s => s.Usuario)
            .WithMany(u => u.Sessoes)
            .HasForeignKey(s => s.IdUsuario);

        #endregion
    }

}