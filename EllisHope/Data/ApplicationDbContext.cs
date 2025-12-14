using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EllisHope.Models.Domain;

namespace EllisHope.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Domain tables
    public DbSet<Page> Pages { get; set; }
    public DbSet<ContentSection> ContentSections { get; set; }
    public DbSet<Media> MediaLibrary { get; set; }
    public DbSet<MediaUsage> MediaUsages { get; set; }
    public DbSet<ImageSize> ImageSizes { get; set; }
    public DbSet<PageImage> PageImages { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Cause> Causes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure composite key for BlogPostCategories
        builder.Entity<BlogPostCategory>()
            .HasKey(bc => new { bc.BlogPostId, bc.CategoryId });

        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogPost)
            .WithMany(b => b.BlogPostCategories)
            .HasForeignKey(bc => bc.BlogPostId);

        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogCategory)
            .WithMany(c => c.BlogPostCategories)
            .HasForeignKey(bc => bc.CategoryId);

        // Media relationships
        builder.Entity<MediaUsage>()
            .HasOne(mu => mu.Media)
            .WithMany(m => m.Usages)
            .HasForeignKey(mu => mu.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        builder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        builder.Entity<Event>()
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder.Entity<Event>()
            .HasIndex(e => e.EventDate);

        builder.Entity<Media>()
            .HasIndex(m => m.Source);

        builder.Entity<Media>()
            .HasIndex(m => m.Category);

        builder.Entity<MediaUsage>()
            .HasIndex(mu => new { mu.EntityType, mu.EntityId });

        // Cause indexes and configuration
        builder.Entity<Cause>()
            .Property(c => c.GoalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Cause>()
            .Property(c => c.RaisedAmount)
            .HasPrecision(18, 2);

        builder.Entity<Cause>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        builder.Entity<Cause>()
            .HasIndex(c => c.Category);

        builder.Entity<Cause>()
            .HasIndex(c => c.IsPublished);
    }
}
