using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface ICauseService
{
    Task<IEnumerable<Cause>> GetAllCausesAsync(bool includeUnpublished = false);
    Task<IEnumerable<Cause>> GetFeaturedCausesAsync(int count = 4);
    Task<IEnumerable<Cause>> GetActiveCausesAsync();
    Task<Cause?> GetCauseByIdAsync(int id);
    Task<Cause?> GetCauseBySlugAsync(string slug);
    Task<IEnumerable<Cause>> SearchCausesAsync(string searchTerm);
    Task<IEnumerable<Cause>> GetCausesByCategoryAsync(string category);
    Task<IEnumerable<Cause>> GetSimilarCausesAsync(int causeId, int count = 4);
    Task<Cause> CreateCauseAsync(Cause cause);
    Task<Cause> UpdateCauseAsync(Cause cause);
    Task DeleteCauseAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeCauseId = null);
    string GenerateSlug(string title);
}
