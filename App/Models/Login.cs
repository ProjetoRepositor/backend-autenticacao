namespace App.Models;

public class Login
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? CodigoAutenticacao { get; set; }
    public int IdUsuario { get; set; }
}