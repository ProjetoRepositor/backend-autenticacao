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

    public DbSet<Example> example { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString, p => {
            p.EnableRetryOnFailure();
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Example>().Property(b => b.Id).HasColumnName("id").IsRequired().UseIdentityColumn();
        modelBuilder.Entity<Example>().Property(b => b.Palavra).HasColumnName("palavra");
        modelBuilder.Entity<Example>().Property(b => b.Inteiro).HasColumnName("inteiro");
        modelBuilder.Entity<Example>().Property(b => b.Flutuante).HasColumnName("flutuante");
        modelBuilder.Entity<Example>().Property(b => b.DiaMesAno).HasColumnName("dia_mes_ano");
    }

}