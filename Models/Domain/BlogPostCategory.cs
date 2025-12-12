namespace EllisHope.Models.Domain;

public class BlogPostCategory
{
    public int BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;

    public int CategoryId { get; set; }
    public BlogCategory BlogCategory { get; set; } = null!;
}
