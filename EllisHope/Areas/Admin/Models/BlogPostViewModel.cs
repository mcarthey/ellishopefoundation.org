using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EllisHope.Areas.Admin.Models;

public class BlogPostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Slug cannot exceed 300 characters")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Excerpt cannot exceed 500 characters")]
    public string Excerpt { get; set; } = string.Empty;

    public string? FeaturedImageUrl { get; set; }

    [Display(Name = "Featured Image")]
    public IFormFile? FeaturedImageFile { get; set; }

    [Required(ErrorMessage = "Author name is required")]
    [StringLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Published Date")]
    public DateTime? PublishedDate { get; set; }

    [StringLength(200, ErrorMessage = "Meta description cannot exceed 200 characters")]
    [Display(Name = "Meta Description (SEO)")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Categories")]
    public List<int> SelectedCategoryIds { get; set; } = new();

    public List<SelectListItem> AvailableCategories { get; set; } = new();

    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }
}
