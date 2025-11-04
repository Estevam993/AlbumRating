using AlbumRating.Contexts;
using AlbumRating.DTOs;
using AlbumRating.Models;
using AlbumRating.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlbumRating.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(CreateUserDto userDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
        {
            return BadRequest(new { message = "Email já cadastrado" });
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
            message = "Usuário criado com sucesso!",
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                dateCreated = user.DateCreated
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Active);

        if (user == null || !user.VerifyPassword(loginDto.Password))
        {
            return Unauthorized(new { message = "Email ou senha inválidos" });
        }

        var token = TokenServices.GenerateToken(user);

        return Ok(new
        {
            message = "Login realizado com sucesso!",
            token,
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            }
        });
    }
}