using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
  [Route("v1/products")]
  public class ProductController : ControllerBase
  {
    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> Get(
      [FromServices] DataContext context
    )
    {
      // Include faz um join entre tabelas
      var products = await context
        .Products
        .Include(p => p.Category)
        .AsNoTracking()
        .ToListAsync();

      return products;
    }

    [HttpGet]
    [Route("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetById(
      int id,
      [FromServices] DataContext context
    )
    {
      var product = await context
        .Products
        .Include(p => p.Category)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

      if (product == null)
        return NotFound(new { message = "Produto não encontrado." });

      return product;
    }

    [HttpGet]
    [Route("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> GetByCategory(
      int id,
      [FromServices] DataContext context
    )
    {
      var products = await context
        .Products
        .Include(p => p.Category)
        .AsNoTracking()
        .Where(p => p.CategoryId == id)
        .ToListAsync();

      return products;
    }

    [HttpPost]
    [Route("")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Post(
      [FromBody] Product model,
      [FromServices] DataContext context
    )
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        context.Products.Add(model);
        await context.SaveChangesAsync();

        // Seta os dados da categoria ao invés de nulo no produto após a persistência
        var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.CategoryId);
        model.Category = category;

        return Ok(model);
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível criar o produto." });
      }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<Product>> Put(
      int id,
      [FromBody] Product model,
      [FromServices] DataContext context
    )
    {
      if (model.Id != id)
        return NotFound(new { message = "Produto não encontrado." });

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        context.Entry<Product>(model).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return Ok(model);
      }
      catch (DbUpdateConcurrencyException)
      {
        return BadRequest(new { message = "Este registro já foi atualizado." });
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível atualizar o produto." });
      }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<Product>> Delete(
      int id,
      [FromServices] DataContext context
    )
    {
      var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
      if (product == null)
        return NotFound(new { message = "Produto não encontrado." });

      try
      {
        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return Ok(new { message = "Produto removido com sucesso." });
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível remover o produto." });
      }
    }
  }
}