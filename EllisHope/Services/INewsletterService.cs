using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service for managing newsletter subscriptions and sending newsletters
/// </summary>
public interface INewsletterService
{
    #region Subscription Management

    /// <summary>
    /// Subscribe an email address to the newsletter
    /// </summary>
    /// <param name="email">Email address to subscribe</param>
    /// <param name="name">Optional subscriber name</param>
    /// <param name="source">Source of subscription (e.g., "Contact Page")</param>
    /// <param name="ipAddress">IP address for spam prevention</param>
    /// <returns>True if subscribed successfully, false if already subscribed</returns>
    Task<(bool Success, string Message)> SubscribeAsync(string email, string? name = null, string? source = null, string? ipAddress = null);

    /// <summary>
    /// Unsubscribe using the secure unsubscribe token
    /// </summary>
    /// <param name="token">Unsubscribe token from email link</param>
    /// <returns>True if unsubscribed successfully</returns>
    Task<(bool Success, string Message)> UnsubscribeAsync(string token);

    /// <summary>
    /// Get a subscriber by their unsubscribe token
    /// </summary>
    Task<Subscriber?> GetSubscriberByTokenAsync(string token);

    /// <summary>
    /// Get all active (non-unsubscribed) subscribers
    /// </summary>
    Task<List<Subscriber>> GetActiveSubscribersAsync();

    /// <summary>
    /// Get all subscribers including unsubscribed
    /// </summary>
    Task<List<Subscriber>> GetAllSubscribersAsync();

    /// <summary>
    /// Get count of active subscribers
    /// </summary>
    Task<int> GetActiveSubscriberCountAsync();

    /// <summary>
    /// Check if an email is already subscribed
    /// </summary>
    Task<bool> IsSubscribedAsync(string email);

    /// <summary>
    /// Delete a subscriber permanently (admin function)
    /// </summary>
    Task<bool> DeleteSubscriberAsync(int subscriberId);

    #endregion

    #region Newsletter Management

    /// <summary>
    /// Create a new newsletter draft
    /// </summary>
    Task<Newsletter> CreateDraftAsync(string subject, string htmlContent, string? plainTextContent = null);

    /// <summary>
    /// Update an existing draft newsletter
    /// </summary>
    Task<bool> UpdateDraftAsync(int newsletterId, string subject, string htmlContent, string? plainTextContent = null);

    /// <summary>
    /// Get a newsletter by ID
    /// </summary>
    Task<Newsletter?> GetNewsletterByIdAsync(int id);

    /// <summary>
    /// Get all draft newsletters
    /// </summary>
    Task<List<Newsletter>> GetDraftNewslettersAsync();

    /// <summary>
    /// Get all sent newsletters (history)
    /// </summary>
    Task<List<Newsletter>> GetSentNewslettersAsync();

    /// <summary>
    /// Send a newsletter to all active subscribers
    /// </summary>
    /// <param name="newsletterId">Newsletter to send</param>
    /// <param name="senderId">User ID of sender</param>
    /// <returns>Number of recipients</returns>
    Task<(bool Success, int RecipientCount, string Message)> SendNewsletterAsync(int newsletterId, string senderId);

    /// <summary>
    /// Delete a draft newsletter
    /// </summary>
    Task<bool> DeleteDraftAsync(int newsletterId);

    /// <summary>
    /// Generate unsubscribe URL for a subscriber
    /// </summary>
    string GenerateUnsubscribeUrl(string baseUrl, string unsubscribeToken);

    #endregion

    #region Export

    /// <summary>
    /// Export active subscribers as CSV
    /// </summary>
    Task<string> ExportSubscribersAsCsvAsync();

    #endregion
}
