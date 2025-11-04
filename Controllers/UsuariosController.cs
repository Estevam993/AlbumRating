using AlbumRating.DTOs;

namespace AlbumRating.Controllers;

using Contexts;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsuarios()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/usuarios/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUsuario(int id)
    {
        var usuario = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.DateCreated,
            })
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();

        return Ok(usuario);
    }

    // POST: api/usuarios
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserDto userDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
        {
            return BadRequest(new { message = "E-mail already taken!" });
        }

        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            DateCreated = DateTime.Now,
            Active = true
        };

        user.SetPassword(userDto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User created successfully!",
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                dateCreated = user.DateCreated
            }
        });
    }
}