using System.Security.Claims;
using ApiFinanceira.Contexts;
using ApiFinanceira.DTOs;
using ApiFinanceira.Models;
using ApiFinanceira.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiFinanceira.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlbumRatingController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public AlbumRatingController(ApplicationDbContext dbContext, HttpClient httpClient)
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

    [HttpPost]
    public async Task<ActionResult> CreateReview([FromBody] CreateAlbumReviewDto createAlbumReviewDto)
    {
        var userId = GetUserId();

        var userExist = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExist)
        {
            return NotFound(new { message = "User not found." });
        }

        try
        {
            var albumReview = new AlbumReview
            {
                AlbumId = createAlbumReviewDto.AlbumId,
                UserId = userId,
                Rate = createAlbumReviewDto.Rate,
                Review = createAlbumReviewDto.Review
            };

            _dbContext.Add(albumReview);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Album review created!", albumReview });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to save album review.", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult> EditReview([FromBody] UpdateAlbumReviewDto updateAlbumReviewDto)
    {
        var userId = GetUserId();

        var userExist = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExist)
        {
            return NotFound(new { message = "User not found." });
        }

        try
        {
            var existingReview = await _dbContext.AlbumReview
                .FirstOrDefaultAsync(r => r.AlbumId == updateAlbumReviewDto.AlbumId && r.UserId == userId);

            if (existingReview == null)
            {
                return NotFound(new { message = "Album review not found." });
            }

            existingReview.Rate = updateAlbumReviewDto.Rate;
            existingReview.Review = updateAlbumReviewDto.Review;

            _dbContext.AlbumReview.Update(existingReview);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Album review updated!", existingReview });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to save album review.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetMyReviews([FromQuery] string token)
    {
        var userId = GetUserId();

        var userExist = await _dbContext.Users
            .AnyAsync(u => u.Id == userId);
        
        if (!userExist)
        {
            return NotFound(new { message = "User not found." });
        }

        try
        {
            var reviews  = await _dbContext.AlbumReview
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var formatedAlbumRating = await AlbumRatingServices.GetAlbumsInfoSpotify(reviews, token);

            return Ok(formatedAlbumRating);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to get albums reviews.", error = ex.Message });
        }
    }
}