using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

// http://localhost:5000
// https://localhost:5001

[Route("v1/categories")]
public class CategoryController : ControllerBase
{
  [HttpGet]
  [Route("")]
  [AllowAnonymous]
  // Cache por requisição
  [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
  // [ResponseCache(Durantion = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Utilizar essa linha quando o cache estiver habilitado no Startup
  public async Task<ActionResult<List<Category>>> Get(
    [FromServices] DataContext context
  )
  {
    // AsNoTracking serve para quando os dados vão ser utilizados apenas para leitura, sem trazer as infos adicionais
    // ToListAsync executa a query de fato
    var categories = await context.Categories.AsNoTracking().ToListAsync();

    return Ok(categories);
  }

  [HttpGet]
  [Route("{id:int}")]
  [AllowAnonymous]
  public async Task<ActionResult<Category>> GetById(
    int id,
    [FromServices] DataContext context
  )
  {
    // FirstOrDefaultAsync traz o primeiro item de uma lista, seja ela com um ou varios itens, ou retorna nulo caso não encontre
    var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    if (category == null)
      return NotFound(new { message = "Categoria não encontrada." });

    return Ok(category);
  }

  [HttpPost]
  [Route("")]
  [Authorize(Roles = "employee")]
  public async Task<ActionResult<List<Category>>> Post(
    [FromBody] Category model,
    [FromServices] DataContext context
  )
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    try
    {
      context.Categories.Add(model);

      // Persiste os dados no banco EM MEMÓRIA
      await context.SaveChangesAsync();

      return Ok(model);
    }
    catch (Exception)
    {
      return BadRequest(new { message = "Não foi possível criar a categoria." });
    }
  }

  [HttpPut]
  [Route("{id:int}")]
  [Authorize(Roles = "employee")]
  public async Task<ActionResult<List<Category>>> Put(
    int id,
    [FromBody] Category model,
    [FromServices] DataContext context
  )
  {
    // Verifica se o ID informado é o mesmo do modelo
    if (model.Id != id)
      return NotFound(new { message = "Categoria não encontrada." });

    // Verifica se os dados são válidos
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    try
    {
      // Verifica as modificações campo a campo e só persiste o que foi de fato modificado
      context.Entry<Category>(model).State = EntityState.Modified;
      await context.SaveChangesAsync();

      return Ok(model);
    }
    catch (DbUpdateConcurrencyException)
    {
      return BadRequest(new { message = "Este registro já foi atualizado." });
    }
    catch (Exception)
    {
      return BadRequest(new { message = "Não foi possível atualizar a categoria." });
    }
  }

  [HttpDelete]
  [Route("{id:int}")]
  [Authorize(Roles = "employee")]
  public async Task<ActionResult<Category>> Delete(
    int id,
    [FromServices] DataContext context
  )
  {
    var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    if (category == null)
      return NotFound(new { message = "Categoria não encontrada." });

    try
    {
      context.Categories.Remove(category);
      await context.SaveChangesAsync();

      return Ok(new { message = "Categoria removida com sucesso." });
    }
    catch (Exception)
    {
      return BadRequest(new { message = "Não foi possível remover a categoria." });
    }
  }
}