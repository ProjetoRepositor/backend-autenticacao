namespace App.Models;

public class AtualizarContaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; } = DateOnly.MinValue;
    public int IdSexo { get; set; } = 0;
    public string Email { get; set; } = string.Empty;
}