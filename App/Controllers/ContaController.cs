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
        request.Cpf = request.Cpf.Trim();
        request.Email = request.Email.Trim();
        #endregion
        
        #region Validações
        
        _logger.LogInformation($"Requisição recebida, tentando criar conta para o CPF {request.Cpf} com o email {request.Email}");
        
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

        if (!Metodos.ValidaCpf(request.Cpf))
        {
            _logger.LogError($"CPF Inválido {request.Cpf}");
            return ValidationProblem($"CPF Inválido {request.Cpf}");
        }

        if (!Metodos.ValidaIdade(request.DataNascimento, out var idade))
        {
            _logger.LogError($"Usuário precisa ter 18 anos ou mais para utilizar o aplicativo, usuário tem {idade} anos");
            return ValidationProblem($"Você precisa ter 18 anos ou mais para utilizar o aplicativo, sua idade: {idade}");
        }
        
        _logger.LogDebug("Conectando ao banco de dados");
        
        await using var contexto = new Contexto();
        
        _logger.LogInformation("Verificando se Sexo é válido");

        var sexo = await contexto.Sexo.Where(s => s.Id == request.IdSexo).FirstOrDefaultAsync();

        if (sexo == null)
        {
            _logger.LogError($"Sexo com id: {request.IdSexo} não existe");
            return ValidationProblem($"Sexo com id: {request.IdSexo} não existe");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome,
            DataNascimento = request.DataNascimento,
            Cpf = request.Cpf,
            IdSexo = request.IdSexo,
        };
        
        var login = new Login
        {
            Email = request.Email,
            Senha = Metodos.HashSenha(request.Senha),
            Usuario = usuario
        };
        
        _logger.LogInformation($"Verificando se CPF ou Email já estão cadastrados");
        
        var usuarioCheck = await contexto.Usuario.Where(u => u.Cpf == request.Cpf).FirstOrDefaultAsync();

        if (usuarioCheck != null)
        {
            _logger.LogInformation($"O CPF {request.Cpf} já está cadastrado");
            return Conflict("CPF já está cadastrado");
        }

        var loginCheckEntity = await contexto.Login.Where(l => l.Email == request.Email).FirstOrDefaultAsync();

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
            await contexto.Login.AddAsync(login);

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
            CPF = request.Cpf,
            request.DataNascimento,
            request.Email,
            Sexo = sexo.Descricao,
        };
        
        _logger.LogInformation($"A conta para o CPF {request.Cpf} com o email {request.Email} foi criada com sucesso");
        
        #endregion

        return Ok(resposta);
    }

    [HttpPut("", Name = "AlterarConta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AlterarConta([FromBody] AtualizarContaRequest request, [FromHeader] string authorize)
    {
        #region Normalização de dados

        request.Cpf = request.Cpf.Trim();
        request.Nome = request.Nome.Trim();
        request.Email = request.Email.Trim();
        var token = authorize.Replace("Bearer ", "");

        #endregion

        #region Validações

        _logger.LogInformation($"Requisição recebida, tentando criar conta para o CPF {request.Cpf} com o email {request.Email}");
        
        if (!Metodos.ValidaEmail(request.Email))
        {
            _logger.LogError($"Email {request.Email} inválido");
            return ValidationProblem($"Email {request.Email} inválido");
        }

        if (!Metodos.ValidaNome(request.Nome))
        {
            _logger.LogError($"Nome Inválido: {request.Nome}, é necessário informar o sobrenome");
            return ValidationProblem($"Nome Inválido: {request.Nome}, é necessário informar o sobrenome");
        }

        if (!Metodos.ValidaCpf(request.Cpf))
        {
            _logger.LogError($"CPF Inválido {request.Cpf}");
            return ValidationProblem($"CPF Inválido {request.Cpf}");
        }
        
        if (!Metodos.ValidaIdade(request.DataNascimento, out var idade))
        {
            _logger.LogError($"Usuário precisa ter 18 anos ou mais para utilizar o aplicativo, usuário tem {idade} anos");
            return ValidationProblem($"Você precisa ter 18 anos ou mais para utilizar o aplicativo, sua idade: {idade}");
        }
        
        await using var contexto = new Contexto();
        
        _logger.LogInformation("Verificando se Sexo é válido");

        var sexo = await contexto.Sexo.Where(s => s.Id == request.IdSexo).FirstOrDefaultAsync();

        if (sexo == null)
        {
            _logger.LogError($"Sexo com id: {request.IdSexo} não existe");
            return ValidationProblem($"Sexo com id: {request.IdSexo} não existe");
        }

        #endregion

        #region Atualizando dados
        
        _logger.LogInformation("Atualizando informações no banco");

        try
        {
            var sessao = await contexto.Sessao.Where(s => s.HashSessao == token).FirstOrDefaultAsync();

            if (sessao == null)
            {
                return NotFound();
            }

            var id = sessao.IdUsuario;
            
            var usuario = await contexto.Usuario.FindAsync(id);

            if (usuario == null)
            {
                _logger.LogError("Usuário não encontrado");
                return NotFound();
            }
            
            usuario.Nome = request.Nome;
            usuario.DataNascimento = request.DataNascimento;
            usuario.IdSexo = request.IdSexo;
            usuario.Cpf = request.Cpf;

            var login = await contexto.Login.Where(l => l.IdUsuario == id).FirstOrDefaultAsync();
            login!.Email = request.Email;

            await contexto.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Ocorreu um erro ao realizar o update no banco de dados: {e.Message}");
            return Problem($"Ocorreu um erro ao realizar o update no banco de dados: {e.Message}");
        }
        
        _logger.LogInformation($"Dados atualizados para a sessao: {token}");

        #endregion

        return Ok();
    }

    [HttpPut("Senha/{id:int}", Name = "AlterarSenha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaRequest request)
    {
        _logger.LogInformation($"Tentando atualizar senha do usuário {id}");
        
        #region Validando senha

        if (!Metodos.ValidaSenha(request.Senha))
        {
            _logger.LogError($"Senha inválida para o usuário {id}");
        }

        #endregion
        
        #region Buscando Dados

        await using var contexto = new Contexto();

        var login = await contexto.Login.Where(l => l.IdUsuario == id).FirstOrDefaultAsync();

        if (login == null)
        {
            _logger.LogError($"Usuário {id} não encontrado");
            return Problem($"Usuário {id} não encontrado");
        }
        
        #endregion

        #region Atualizando Senha

        login.Senha = Metodos.HashSenha(request.Senha);

        await contexto.SaveChangesAsync();

        #endregion
        
        return Ok();
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
        
        var login = await contexto.Login.Where(
            l => l.Email == request.Email &&
                 l.Senha == Metodos.HashSenha(request.Senha) &&
                 l.Ativo).FirstOrDefaultAsync();
        
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
            await contexto.Sessao.AddAsync(sessao);

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

        var sessao = await contexto.Sessao.Where(s => s.HashSessao == token).FirstOrDefaultAsync();

        if (sessao == null)
        {
            return NotFound();
        }
        
        _logger.LogInformation("Achou a sessao");
        
        #endregion
        
        #region Monta Resposta

        var query = from usuario in contexto.Usuario
            join login in contexto.Login
                on usuario.Id equals login.IdUsuario
            join sexo in contexto.Sexo
                on usuario.IdSexo equals sexo.Id
            where usuario.Id == sessao.IdUsuario
            select new
            {
                CPF = usuario.Cpf,
                usuario.Nome,
                usuario.DataNascimento,
                login.Email,
                Sexo = sexo.Descricao,
            };
        

        var resposta = await query.FirstOrDefaultAsync();
        
        #endregion
        
        return Ok(resposta);
    }

    [HttpGet("{id:int}", Name = "BuscarDetalhesPorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarUsuarioPorId(int id)
    {
        #region Monta Resposta

        await using var contexto = new Contexto();

        var resultado = await contexto.Conta.Where(c => c.Id == id).FirstOrDefaultAsync();

        #endregion

        if (resultado == null)
        {
            return NotFound();
        }

        return Ok(resultado);
    }

    [HttpDelete(Name = "DesativarConta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DesativarConta([FromHeader] string authorize)
    {
        #region Busca Conta
        
        _logger.LogInformation($"Tentando desativar conta vinculada à sessão: {authorize}");

        var token = authorize.Replace("Bearer ", "");
        
        await using var contexto = new Contexto();

        var sessao = await contexto.Sessao.Where(s => s.HashSessao == token).FirstOrDefaultAsync();

        if (sessao == null)
        {
            _logger.LogError($"Sessão {authorize} não encontrada");
            return NotFound();
        }
        
        _logger.LogInformation("Achou a sessao");
        
        #endregion

        #region Desativando Conta

        var loginQuery = contexto.Login.Where(l => l.IdUsuario == sessao.IdUsuario);

        var login = await loginQuery.FirstOrDefaultAsync();

        if (login == null)
        {
            _logger.LogCritical($"Existe uma sessão sem usuário ou login, verifique o banco de dados urgentemente, sessão: {sessao.HashSessao}");
            return Problem();
        }

        if (!login.Ativo)
        {
            _logger.LogError($"Usuário com sessão {sessao.HashSessao} já está desativado");
            return BadRequest();
        }

        login.Ativo = false;

        await contexto.SaveChangesAsync();

        #endregion
        
        return Ok();
    }

    [HttpPost("SolicitarRecupercaorSenha", Name = "Solicitar Recuperacao Senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SolcitarRecuperacaoSenha([FromBody] SolicitarRecuperacaoSenhaRequest request)
    {
        
        #region Buscando email na base
        
        await using var contexto = new Contexto();

        var login = await contexto.Login.Where(l => l.Email == request.Email).FirstOrDefaultAsync();

        if (login == null)
        {
            return NotFound();
        }

        #endregion

        #region Insere código de recuperação

        var recuperacaoSenha = new RecuperacaoSenha
        {
            HashRecuperacao = Metodos.HashSenha($"{DateTime.UtcNow}:{request.Email}"),
            IdLogin = login.Id,
        };

        try
        {
            await contexto.RecuperacaoSenha.AddAsync(recuperacaoSenha);

            await contexto.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Ocorreu um erro ao inserir as informações: {e.InnerException.Message}");
            return Problem($"Ocorreu um erro ao inserir as informações: {e.Message}");
        }

        #endregion

        return Ok();
    }

    [HttpPost("RecuperarSenha", Name = "Recuperar Senha"),]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaRequest request)
    {
        #region Validando senha

        if (!Metodos.ValidaSenha(request.Senha))
        {
            return BadRequest("Senha Invalida");
        }
            
        #endregion
        
        
        #region Buscando dados

        await using var contexto = new Contexto();

        var recuperacaoSenha = await contexto.RecuperacaoSenha.Where(r => r.HashRecuperacao == request.HashRecuperacao)
            .FirstOrDefaultAsync();

        if (recuperacaoSenha == null)
        {
            return NotFound();
        }

        var login = await contexto.Login.Where(l => l.Id == recuperacaoSenha.IdLogin).FirstOrDefaultAsync();

        if (login == null)
        {
            return Problem();
        }
        
        #endregion

        #region Atualizando dados

        login.Senha = Metodos.HashSenha(request.Senha);

        await contexto.RecuperacaoSenha.Where(r => r.Id == recuperacaoSenha.Id).ExecuteDeleteAsync();

        await contexto.SaveChangesAsync();
        
        #endregion
        
        return Ok();
    }
}
