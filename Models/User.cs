using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlbumRating.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; }

    [Required] public string Password { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.Now;

    public bool Active { get; set; } = true;


    public ICollection<AlbumReview> AlbumReviews { get; set; } = new List<AlbumReview>();

    public void SetPassword(string password)
    {
        Password = BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, Password);
    }
}