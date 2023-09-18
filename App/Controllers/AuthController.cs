using App.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers;

[Route("/api/autenticacao/v1/[controller]")]
public class AuthController: ControllerBase
{
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpHead]
    [HttpOptions]
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Autenticar([FromHeader] string authorize)
    {
        #region Verificando validade da sessão

        var token = authorize.Replace("Bearer ", "");
        
        await using var contexto = new Contexto();

        var sessao = await contexto.sessao.Where(s => s.HashSessao == token).FirstOrDefaultAsync();

        if (
            sessao != null &&
            (
                sessao.ManterLogin ||
                sessao.UltimoAcesso > DateTime.Now.AddHours(-3)
            )
        ) return Ok();

        return Unauthorized();
        
        #endregion
    }
}