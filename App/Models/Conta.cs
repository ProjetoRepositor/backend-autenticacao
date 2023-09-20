namespace App.Models;

public class Conta
{
    public int Id { get; set; } = 0;
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; } = DateOnly.MinValue;
    public string Sexo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}