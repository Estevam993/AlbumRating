using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AlbumRating.Models;

namespace AlbumRating.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<AlbumReview> AlbumReview { get; set; }
}