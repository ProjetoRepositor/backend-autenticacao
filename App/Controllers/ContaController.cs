using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace App.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using DB;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("/api/autenticacao/v1/[controller]")]
public class ContaController : ControllerBase
{
    private readonly ILogger<ContaController> _logger;

    public ContaController(ILogger<ContaController> logger)
    {
        _logger = logger;
    }

    private static string HashSenha(string senha)
    {
        byte[] salt = { 5, 10, 15, 20, 25, 30, 35 };
        
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: senha,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed;
    }

    [HttpPost(Name = "CriarConta")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
    {
        
        _logger.LogInformation($"Requisição recebida, tentando criar conta para o CPF {request.CPF} com o email {request.Email}");
        
        _logger.LogDebug("Conectando ao banco de dados");
        
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
            Usuario = usuario
        };
        
        _logger.LogInformation($"Verificando se CPF ou Email já estão cadastrados");

        var usuarioCheckQuery = from usuarioCheck in contexto.usuario
            where usuario.CPF == usuarioCheck.CPF
            select usuarioCheck;

        var loginCheckQuery = from loginCheck in contexto.login
            where login.Email == loginCheck.Email
            select loginCheck;
        
        var usuarioCheckEntity = await usuarioCheckQuery.FirstOrDefaultAsync();

        if (usuarioCheckEntity != null)
        {
            _logger.LogInformation($"O CPF {request.CPF} já está cadastrado");
            return Conflict("CPF já está cadastrado");
        }

        var loginCheckEntity = await loginCheckQuery.FirstOrDefaultAsync();

        if (loginCheckEntity != null)
        {
            _logger.LogInformation($"O Email {request.Email} já está em uso");
            return Conflict("Email já está em uso");
        }
        
        _logger.LogInformation("Inserindo Informações");

        try
        {
            await contexto.login.AddAsync(login);

            await contexto.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Ocorreu um erro ao inserir as informações: {e.Message}");
        }

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
}
