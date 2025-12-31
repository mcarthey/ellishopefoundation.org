using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface ITestimonialService
{
    // Public queries
    Task<IEnumerable<Testimonial>> GetFeaturedTestimonialsAsync(int count = 5);
    Task<IEnumerable<Testimonial>> GetPublishedTestimonialsAsync();

    // Admin queries
    Task<IEnumerable<Testimonial>> GetAllTestimonialsAsync(bool includeUnpublished = true);
    Task<IEnumerable<Testimonial>> GetPendingApprovalAsync();
    Task<Testimonial?> GetByIdAsync(int id);

    // CRUD
    Task<Testimonial> CreateAsync(Testimonial testimonial, string userId, bool canAutoApprove);
    Task<Testimonial> UpdateAsync(Testimonial testimonial);
    Task<bool> DeleteAsync(int id);

    // Actions
    Task<bool> PublishAsync(int id);
    Task<bool> UnpublishAsync(int id);
    Task<bool> FeatureAsync(int id, bool featured);
    Task<bool> ApproveAsync(int id, string approverId);
    Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
}
