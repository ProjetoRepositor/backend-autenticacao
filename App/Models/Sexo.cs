namespace App.Models;

public class Sexo
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    
    public List<Usuario> Usuarios { get; set; } = null!;
}