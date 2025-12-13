using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    [MaxLength(500)]
    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    public int? FeaturedImageId { get; set; }
    public Media? FeaturedImage { get; set; }

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    [MaxLength(256)]
    public string? AuthorId { get; set; }

    [MaxLength(200)]
    public string? AuthorName { get; set; }

    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; } = false;

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public ICollection<BlogPostCategory> BlogPostCategories { get; set; } = new List<BlogPostCategory>();
}
