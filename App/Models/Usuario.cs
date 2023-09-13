namespace App.Models;


public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    
    public DateOnly DataNascimento { get; set; }
    public int IdSexo { get; set; }
    
    public Login Login { get; set; }
    public Sexo Sexo { get; set; }
    public List<Sessao> Sessoes { get; set; }
}
