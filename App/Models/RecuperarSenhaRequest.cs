namespace App.Models;

public class RecuperarSenhaRequest
{
    public string Senha { get; set; } = string.Empty;
    public string HashRecuperacao { get; set; } = string.Empty;
}