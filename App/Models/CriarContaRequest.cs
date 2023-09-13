namespace App.Models;

public class CriarContaRequest
{
    public string Nome { get; set; }
    public string CPF { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int IdSexo { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
}