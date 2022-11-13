using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
  [Route("v1/users")]
  public class UserController : Controller
  {
    [HttpGet]
    [Route("")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<List<User>>> Get(
      [FromServices] DataContext context
    )
    {
      var users = await context.Users.AsNoTracking().ToListAsync();

      return users;
    }

    [HttpPost]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Post(
      [FromServices] DataContext context,
      [FromBody] User model
    )
    {
      // Verifica se os dados são válidos
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        // Força o usuário a ser sempre "funcionário"
        model.Role = "employee";

        context.Users.Add(model);
        await context.SaveChangesAsync();

        // Esconde a senha
        model.Password = "";

        return model;
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível criar o usuário." });
      }
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<dynamic>> Authenticate(
      [FromServices] DataContext context,
      [FromBody] User model
    )
    {
      var user = await context.Users.AsNoTracking().Where(u => u.Username == model.Username && u.Password == model.Password).FirstOrDefaultAsync();

      if (user == null)
        return NotFound(new { message = "Usuário ou senha inválidos." });

      var token = TokenService.GenerateToken(user);

      // Esconde a senha
      model.Password = "";

      return new
      {
        user = user,
        token = token
      };
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Put(
      int id,
      [FromBody] User model,
      [FromServices] DataContext context
    )
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (id != model.Id)
        return NotFound(new { message = "Usuário não encontrado." });

      try
      {
        context.Entry(model).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return model;
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível criar o usuário." });
      }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<ActionResult<User>> Delete(
      int id,
      [FromServices] DataContext context
    )
    {
      var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
        return NotFound(new { message = "Usuário não encontrado." });

      try
      {
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return Ok(new { message = "Usuário removido com sucesso." });
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Não foi possível remove o usuário." });
      }

    }
  }
}