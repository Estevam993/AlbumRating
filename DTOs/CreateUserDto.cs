using System.ComponentModel.DataAnnotations;

namespace AlbumRating.DTOs;

public class CreateUserDto
{
    [Required] [StringLength(100)] public string Name { get; set; }

    [Required] [EmailAddress] public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "As senhas não conferem")]
    public string ConfirmPassword { get; set; }
}