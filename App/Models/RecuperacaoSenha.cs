namespace App.Models;

public class RecuperacaoSenha
{
    public int Id { get; set; }
    public string HashRecuperacao { get; set; } = string.Empty;
    public int IdLogin { get; set; }
}