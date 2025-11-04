using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlbumRating.DTOs;

public class CreateAlbumReviewDto
{
    [Range(0, 100)]
    public int Rate { get; set; }         
    
    [Column(TypeName = "BLOB")]
    public string Review { get; set; }     
    
    [Required]
    public string AlbumId { get; set; }    
    
}