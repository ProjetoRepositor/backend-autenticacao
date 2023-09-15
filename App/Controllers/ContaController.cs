namespace App.Controllers;

using Microsoft.AspNetCore.Mvc;
using Models;
using DB;
using Services;
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

    [HttpPost("Criar", Name = "CriarConta")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
    {
        #region Normalização de dados
        request.Nome = request.Nome.Trim();
        request.CPF = request.CPF.Trim();
        request.Email = request.Email.Trim();
        #endregion
        
        #region Validações
        
        _logger.LogInformation($"Requisição recebida, tentando criar conta para o CPF {request.CPF} com o email {request.Email}");
        
        if (!Metodos.ValidaEmail(request.Email))
        {
            _logger.LogError($"Email {request.Email} inválido");
            return ValidationProblem($"Email {request.Email} inválido");
        }

        if (!Metodos.ValidaSenha(request.Senha))
        {
            _logger.LogError("Senha inválida");
            return ValidationProblem("Senha Inválida");
        }

        if (!Metodos.ValidaNome(request.Nome))
        {
            _logger.LogError($"Nome Inválido: {request.Nome}, é necessário informar o sobrenome");
            return ValidationProblem($"Nome Inválido: {request.Nome}, é necessário informar o sobrenome");
        }

        if (!Metodos.ValidaCPF(request.CPF))
        {
            _logger.LogError($"CPF Inválido {request.CPF}");
            return ValidationProblem($"CPF Inválido {request.CPF}");
        }

        if (!Metodos.ValidaIdade(request.DataNascimento))
        {
            _logger.LogError("Você precisa ter 18 anos ou mais para utilizar o aplicativo");
            return ValidationProblem("Você precisa ter 18 anos ou mais para utilizar o aplicativo");
        }
        
        _logger.LogDebug("Conectando ao banco de dados");
        
        await using var contexto = new Contexto();
        
        _logger.LogInformation("Verificando se Sexo é válido");

        var sexo = await contexto.sexo.Where(s => s.Id == request.IdSexo).FirstOrDefaultAsync();

        if (sexo == null)
        {
            _logger.LogError($"Sexo com id: {request.IdSexo} não existe");
            return ValidationProblem($"Sexo com id: {request.IdSexo} não existe");
        }

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
            Senha = Metodos.HashSenha(request.Senha),
            Usuario = usuario
        };
        
        _logger.LogInformation($"Verificando se CPF ou Email já estão cadastrados");
        
        var usuarioCheck = await contexto.usuario.Where(u => u.CPF == request.CPF).FirstOrDefaultAsync();

        if (usuarioCheck != null)
        {
            _logger.LogInformation($"O CPF {request.CPF} já está cadastrado");
            return Conflict("CPF já está cadastrado");
        }

        var loginCheckEntity = await contexto.login.Where(l => l.Email == request.Email).FirstOrDefaultAsync();

        if (loginCheckEntity != null)
        {
            _logger.LogInformation($"O Email {request.Email} já está em uso");
            return Conflict("Email já está em uso");
        }
        
        #endregion
        
        #region Inserção
        
        _logger.LogInformation("Inserindo Informações");
        
        try
        {
            await contexto.login.AddAsync(login);

            await contexto.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Ocorreu um erro ao inserir as informações: {e.Message}");
            return Problem($"Ocorreu um erro ao inserir as informações: {e.Message}");
        }
        
        #endregion
        
        #region Monta Resposta

        var resposta = new
        {
            request.Nome,
            request.CPF,
            request.DataNascimento,
            request.Email,
            Sexo = sexo.Descricao,
        };
        
        _logger.LogInformation($"A conta para o CPF {request.CPF} com o email {request.Email} foi criada com sucesso");
        
        #endregion

        return Ok(resposta);
    }

    [HttpPost("Login", Name = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        #region Normalização dos dados

        request.Email = request.Email.Trim();

        #endregion
        
        #region Validações
        
        _logger.LogInformation($"Tentando login para email {request.Email}");
        
        await using var contexto = new Contexto();
        
        var login = await contexto.login.Where(l => l.Email == request.Email && l.Senha == Metodos.HashSenha(request.Senha)).FirstOrDefaultAsync();
        
        if (login == null)
        {
            _logger.LogError($"Login falhou para o email {request.Email}");
            return NotFound(new { mensagem = $"Login falhou para o email {request.Email}" });
        }
        
        #endregion

        #region Inserção
        
        var timestamp = DateTime.UtcNow.Ticks;

        var hashSessao = Metodos.HashSenha($"{login.Id}:{timestamp}");

        var sessao = new Sessao
        {
            HashSessao = hashSessao,
            ManterLogin = request.ManterLogin,
            UltimoAcesso = DateTime.UtcNow,
            IdUsuario = login.IdUsuario,
        };
        
        try
        {
            await contexto.sessao.AddAsync(sessao);

            await contexto.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Ocorreu um erro ao inserir as informações: {e.Message}");
            return Problem($"Ocorreu um erro ao inserir as informações: {e.Message}");
        }
        
        #endregion

        #region Monta Resposta

        var resposta = new
        {
            sessao.HashSessao,
            sessao.ManterLogin,
            sessao.UltimoAcesso
        };
        
        #endregion

        return Created("", resposta);
    }

    [HttpGet(Name = "BuscarUsuarioPorSessao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BuscaUsuarioPorSessao([FromHeader] string authorize)
    {
        #region Normalização dos dados

        var token = authorize.Replace("Bearer ", "");

        #endregion
        
        #region Validações
        
        await using var contexto = new Contexto();

        var sessao = await contexto.sessao.Where(s => s.HashSessao == token).FirstOrDefaultAsync();

        if (sessao == null)
        {
            return NotFound();
        }
        
        _logger.LogInformation("Achou a sessao");
        
        #endregion
        
        #region Monta Resposta

        var query = from usuario in contexto.usuario
            join login in contexto.login
                on usuario.Id equals login.IdUsuario
            join sexo in contexto.sexo
                on usuario.IdSexo equals sexo.Id
            where usuario.Id == sessao.IdUsuario
            select new
            {
                usuario.CPF,
                usuario.Nome,
                usuario.DataNascimento,
                login.Email,
                Sexo = sexo.Descricao,
            };
        

        var resposta = await query.FirstOrDefaultAsync();
        
        #endregion
        
        return Ok(resposta);
    }

    [HttpGet("{id}", Name = "BuscarDetalhesPorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        #region Monta Resposta
        
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
        
        #endregion

        if (resultado == null)
        {
            return NotFound();
        }
        
        return Ok(resultado);
    }
}
