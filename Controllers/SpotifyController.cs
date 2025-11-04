using System.Text.Json;
using AlbumRating.DTOs;
using AlbumRating.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;

namespace AlbumRating.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotifyController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _spotifyClientId;
    private readonly string _spotifyClientSecret;

    public SpotifyController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _spotifyClientId = Env.GetString("SPOTIFY_CLIENT_ID");
        _spotifyClientSecret = Env.GetString("SPOTIFY_CLIENT_SECRET");
    }

    public class SpotifyTokenResponse
    {
        public string Access_Token { get; set; } = string.Empty;
        public string Token_Type { get; set; } = string.Empty;
        public int Expires_In { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        try
        {
            var requestData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", _spotifyClientId),
                new("client_secret", _spotifyClientSecret)
            };

            var content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", content);

            if (response.IsSuccessStatusCode)
            {
                var data = await SpotifyServices.FormatResponse(response);

                return Ok(data);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new
            {
                errorContent
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult> Search([FromQuery] string token, [FromBody] SearchDto searchDto)
    {
        var search = searchDto.Search;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(search))
        {
            return BadRequest(new { message = "Some parameters are missing." });
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        search = SpotifyServices.ToUrlEncoded(search);

        var response =
            await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={search}&type=artist%2Calbum%2Ctrack");

        var formatedData = await SpotifyServices.SearchResponse(response);

        return Ok(formatedData);
    }
}