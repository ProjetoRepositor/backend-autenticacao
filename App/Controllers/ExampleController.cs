namespace App.Controllers;

using Microsoft.AspNetCore.Mvc;
using App.Models;
using App.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

[ApiController]
[Route("/api/v1/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ILogger<ExampleController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetExample")]
    [ProducesResponseType(typeof(IEnumerable<Example>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        using var contexto = new Contexto();

        var queryable = from exemplo in contexto.example
                        select exemplo;

        var response = await queryable.ToListAsync();

        return Ok(response);
    }

    [HttpGet("{id}", Name = "GetExampleById")]
    [ProducesResponseType(typeof(Example), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        using var contexto = new Contexto();

        var query = from exemplo in contexto.example
                    where exemplo.Id == id
                    select exemplo;

        var resultado = await query.FirstOrDefaultAsync();

        if (resultado == null)
        {
            return NotFound();
        }
        return Ok(resultado);
    }

    [HttpPost(Name = "PostExample")]
    [ProducesResponseType(typeof(Example), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] Example example)
    {
        using var contexto = new Contexto();

        if (example == null)
        {
            return BadRequest();
        }
        
        await contexto.example.AddAsync(example);

        await contexto.SaveChangesAsync();

        Response.StatusCode = StatusCodes.Status201Created;

        return Created("", example);
    }

    [HttpPut("{id}", Name = "PutExample")]
    [ProducesResponseType(typeof(Example), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(int id, [FromBody] Example example)
    {

        if (example == null)
        {
            return BadRequest();
        }

        using var contexto = new Contexto();

        var query = from exemplo in contexto.example
                    where exemplo.Id == id
                    select exemplo;

        var atual = await query.FirstAsync();

        if (atual == null)
        {
            return NotFound();
        }

        example.Id = id;

        contexto.example.Entry(atual).State = EntityState.Detached;
        
        contexto.example.Update(example);

        await contexto.SaveChangesAsync();

        return Ok(example);
    }

    [HttpDelete("{id}", Name = "DeleteExample")]
    [ProducesResponseType(typeof(Example), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        using var contexto = new Contexto();

        var obj = await contexto.example.FindAsync(id);

        if (obj != null)
        {
            contexto.example.Remove(obj);
            await contexto.SaveChangesAsync();
        }

        else
        {
            return NotFound();
        }

        return Ok(obj);
    }
}
