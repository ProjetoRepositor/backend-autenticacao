using System.Collections;

namespace App.Models;

public class Sessao
{
    public int Id { get; set; }
    public string HashSessao { get; set; }
    public bool ManterLogin { get; set; }
    public DateTime UltimoAcesso { get; set; }
    public int IdUsuario { get; set; }
    
    public Usuario Usuario { get; set; }
}