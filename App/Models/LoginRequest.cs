namespace App.Models;

public class LoginRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
    public bool ManterLogin { get; set; }
}