namespace App.Models;

public class Sexo
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    
    public List<Usuario> Usuarios { get; set; }
}