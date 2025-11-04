using System.Security.Claims;
using AlbumRating.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlbumRating.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public DashboardController(ApplicationDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    private int GetUserId()
    {
        if (!User.Identity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                          User.FindFirst("userId") ??
                          User.FindFirst(ClaimTypes.Sid);

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            throw new UnauthorizedAccessException("User ID não encontrado no token");
        }

        if (int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID inválido no token");
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var userId = GetUserId();

        try
        {
            var userExist = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Include(u => u.AlbumReviews)
                .ToListAsync();

            if (userExist.Count == 0)
            {
                return NotFound(new { message = "User not found." });
            }

            var user = userExist[0];

            var reviewsCount = user.AlbumReviews.Count(a => a.Review != "");
            var ratingCount = user.AlbumReviews.Count(a => a.Rate != 0);
            var allReviews = user.AlbumReviews
                .Select(a => new
                {
                    a.AlbumId,
                    a.Rate,
                    a.Review,
                    a.DateCreated
                })
                .OrderByDescending(a => a.Rate)
                .ToList();

            return Ok(new
                {
                    Username = user.Name,
                    UserId = user.Id,
                    Email = user.Email,
                    Reviews = new
                    {
                        ReviewsCount = reviewsCount,
                        RatingCount = ratingCount,
                        RatingAverage = user.AlbumReviews.Average(a => a.Rate),
                        Reviews = allReviews
                    }
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to get dashboard.", error = ex.Message });
        }
    }
}