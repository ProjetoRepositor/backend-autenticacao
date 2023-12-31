namespace App.Models;


public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    
    public DateOnly DataNascimento { get; set; }
    public int IdSexo { get; set; }

    public Login Login { get; set; } = null!;
    public Sexo Sexo { get; set; } = null!;
    public List<Sessao> Sessoes { get; set; } = null!;
}
