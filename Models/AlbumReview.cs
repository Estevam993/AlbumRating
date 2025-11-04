using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlbumRating.Models;

public class AlbumReview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public string AlbumId { get; set; }    
    
    [Required]
    [Range(0, 100)]
    public int Rate { get; set; }         
    
    [Required]
    [Column(TypeName = "BLOB")]
    public string Review { get; set; }     
    
    public DateTime DateCreated { get; set; } = DateTime.Now;
    
    public DateTime DateUpdated{ get; set; } = DateTime.Now;
    
    public int UserId { get; set; }
    
    public User User { get; set; }
}
