using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace App.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using DB;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("/api/autenticacao/v1/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(ILogger<UsuarioController> logger)
    {
        _logger = logger;
    }

    private static string HashSenha(string senha)
    {
        byte[] salt = { 5, 10, 15, 20, 25, 30, 35 };
        
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: senha!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed;
    }

    [HttpPost(Name = "CriarConta")]
    [ProducesResponseType(typeof(IEnumerable<Usuario>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
    {
        await using var contexto = new Contexto();

        var usuario = new Usuario
        {
            Nome = request.Nome,
            DataNascimento = request.DataNascimento,
            CPF = request.CPF,
            IdSexo = request.IdSexo,
        };
        
        var login = new Login
        {
            Email = request.Email,
            Senha = HashSenha(request.Senha),
        };

        var usuarioCheckQuery = from usuarioCheck in contexto.usuario
            where usuario.CPF == usuarioCheck.CPF
            select usuarioCheck;

        var loginCheckQuery = from loginCheck in contexto.login
            where login.Email == loginCheck.Email
            select loginCheck;
        
        var usuarioCheckEntity = await usuarioCheckQuery.FirstOrDefaultAsync();

        if (usuarioCheckEntity != null)
        {
            return Conflict();
        }

        var loginCheckEntity = await loginCheckQuery.FirstOrDefaultAsync();

        if (loginCheckEntity != null)
        {
            return Conflict();
        }
        
        await contexto.usuario.AddAsync(usuario);
        
        await contexto.SaveChangesAsync();

        var usuarioQuery = from usuarioNovo in contexto.usuario
            where usuarioNovo.CPF == usuario.CPF
            select usuarioNovo.Id;

        var usuarioNovoId = await usuarioQuery.FirstOrDefaultAsync();

        login.IdUsuario = usuarioNovoId;

        await contexto.login.AddAsync(login);

        await contexto.SaveChangesAsync();
        
        var descricaoSexoQuery = from sexo in contexto.sexo
                            where sexo.Id == request.IdSexo
                            select sexo.Descricao;

        var descricaoSexo = await descricaoSexoQuery.FirstOrDefaultAsync();

        var response = new
        {
            usuario.Nome,
            usuario.CPF,
            usuario.DataNascimento,
            login.Email,
            Sexo = descricaoSexo,
        };

        return Ok(response);
    }

    [HttpGet("{id}", Name = "BuscarDetalhesPorId")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        await using var contexto = new Contexto();

        var query = from usuario in contexto.usuario
            join sexo in contexto.sexo
                on usuario.IdSexo equals sexo.Id
            join login in contexto.login
                on usuario.Id equals login.IdUsuario
            select new
            {
                usuario.Nome,
                usuario.DataNascimento,
                usuario.CPF,
                sexo = sexo.Descricao,
                login.Email,
            };

        var resultado = await query.FirstOrDefaultAsync();

        if (resultado == null)
        {
            return NotFound();
        }
        return Ok(resultado);
    }

    [HttpPut("{id}", Name = "PutExample")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
    {
        await using var contexto = new Contexto();

        var query = from exemplo in contexto.usuario
                    where exemplo.Id == id
                    select exemplo;

        var atual = await query.FirstAsync();

        if (atual == null)
        {
            return NotFound();
        }

        usuario.Id = id;

        contexto.usuario.Entry(atual).State = EntityState.Detached;
        
        contexto.usuario.Update(usuario);

        await contexto.SaveChangesAsync();

        return Ok(usuario);
    }

    [HttpDelete("{id}", Name = "DeleteExample")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await using var contexto = new Contexto();

        var obj = await contexto.usuario.FindAsync(id);

        if (obj != null)
        {
            contexto.usuario.Remove(obj);
            await contexto.SaveChangesAsync();
        }

        else
        {
            return NotFound();
        }

        return Ok(obj);
    }
}
