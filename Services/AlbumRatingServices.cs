using System.Text.Json;
using ApiFinanceira.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiFinanceira.Services;

public class AlbumRatingServices
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public class Album
    {
        public string album_type { get; set; }
        public int total_tracks { get; set; }
        public List<string> available_markets { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public List<Artist> artists { get; set; }
        public Tracks tracks { get; set; }
        public List<Copyright> copyrights { get; set; }
        public ExternalIds external_ids { get; set; }
        public List<object> genres { get; set; }
        public string label { get; set; }
        public int popularity { get; set; }
    }

    public class Artist
    {
        public ExternalUrls external_urls { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Copyright
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class ExternalIds
    {
        public string upc { get; set; }
    }

    public class ExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class Item
    {
        public List<Artist> artists { get; set; }
        public List<string> available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public object preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public bool is_local { get; set; }
    }

    public class Root
    {
        public List<Album> albums { get; set; }
    }

    public class Tracks
    {
        public string href { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
        public List<Item> items { get; set; }
    }

    public class FormatedAlbumsReviews
    {
        public int Id { get; set; }
        public string AlbumId { get; set; }
        public string Review { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public int Rate { get; set; }
        public string Image { get; set; }
        public ArtistInfo Artists { get; set; }
    }

    public class ArtistInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }

    public static async Task<object> GetAlbumsInfoSpotify(List<AlbumReview> reviews, string token)
    {
        var albumsIds = string.Join(",", reviews.Select(r => r.AlbumId).Distinct());

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


        var response =
            await _httpClient.GetAsync($"https://api.spotify.com/v1/albums?ids={albumsIds}");

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

            if (formatedData != null)
                return FormatAlbumsReview(reviews, formatedData);

            throw new Exception($"An Error ocurred in the JSON format.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Spotify API error: {response.StatusCode}, details: {ex.Message}");
        }
    }


    private static List<FormatedAlbumsReviews> FormatAlbumsReview(List<AlbumReview> reviews, Root formatedData)
    {
        var newAlbumsReviews = reviews.Select(r =>
        {
            var thisAlbum = formatedData.albums.FirstOrDefault(a => a.id == r.AlbumId);

            return new FormatedAlbumsReviews
            {
                Id = r.Id,
                AlbumId = r.AlbumId,
                Name = thisAlbum.name,
                Link = thisAlbum.external_urls.spotify,
                Review = r.Review,
                Rate = r.Rate,
                Image = thisAlbum?.images?.FirstOrDefault()?.url,
                Artists = new ArtistInfo
                {
                    Id = thisAlbum?.artists?.FirstOrDefault()?.id,
                    Name = thisAlbum?.artists?.FirstOrDefault()?.name,
                    Link = thisAlbum?.artists?.FirstOrDefault()?.external_urls?.spotify
                }
            };
        }).ToList();

        return newAlbumsReviews;
    }
}