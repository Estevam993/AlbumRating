using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AlbumRating.Services;

public class SpotifyServices
{
    public class SpotifyTokenResponse
    {
        public string Access_Token { get; set; } = string.Empty;
        public string Token_Type { get; set; } = string.Empty;
        public int Expires_In { get; set; }
    }

    public class ExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Image
    {
        public int height { get; set; }
        public int width { get; set; }
        public string url { get; set; }
    }

    public class Artist
    {
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public List<string> genres { get; set; }
        public List<Image> images { get; set; }
    }

    public class Artists
    {
        public string href { get; set; }
        public List<Artist> items { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class Album
    {
        public string album_type { get; set; }
        public List<Artist> artists { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string id { get; set; }
        public string uri { get; set; }
    }

    public class Albums
    {
        public string href { get; set; }
        public List<Album> items { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class Track
    {
        public Album album { get; set; }
        public List<Artist> artists { get; set; }
        public string name { get; set; }
        public int duration_ms { get; set; }
        public string id { get; set; }
        public string uri { get; set; }
    }

    public class Tracks
    {
        public string href { get; set; }
        public List<Track> items { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class Root
    {
        public Tracks tracks { get; set; }
        public Artists artists { get; set; }
        public Albums albums { get; set; }
    }

    public static async Task<SpotifyTokenResponse> FormatResponse(HttpResponseMessage response)
    {
        var data = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(data, options);

        return tokenResponse;
    }

    public static async Task<object> SearchResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Spotify API error: {response.StatusCode}, details: {error}");
        }

        try
        {
            var data = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var formatedData = JsonSerializer.Deserialize<Root>(data, options);

            var responseData = FormatDataToResponse(formatedData);

            return responseData;
        }
        catch (Exception ex)
        {
            throw new Exception($"Spotify API error: {response.StatusCode}, details: {ex.Message}");
        }
    }

    private static object FormatDataToResponse(Root data)
    {
        var artists = data.artists.items.Select(a => new
        {
            a.id,
            a.name,
            Image = a.images.FirstOrDefault()?.url
        }).ToList();

        var tracks = data.tracks.items.Select(t => new
        {
            t.id,
            t.name,
            image = t.album.images.FirstOrDefault()?.url,
            album = t.album.name,
            artist = t.artists.FirstOrDefault()?.name
        }).ToList();

        var albums = data.albums.items.Select(a => new
        {
            a.id,
            a.name,
            artist = a.artists.FirstOrDefault()?.name,
            image = a.images.FirstOrDefault()?.url,
        }).ToList();


        return new
        {
            Artists = artists,
            Tracks = tracks,
            Albums = albums
        };
    }


    public static string ToUrlEncoded(string value)
    {
        return Uri.EscapeDataString(value);
    }
}