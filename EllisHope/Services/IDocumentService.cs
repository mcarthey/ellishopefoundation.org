namespace EllisHope.Services;

/// <summary>
/// Service interface for managing document attachments
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Upload a document and link to application
    /// </summary>
    Task<(bool Succeeded, string[] Errors, string? FilePath)> UploadDocumentAsync(
        int applicationId,
        string documentType,
        Stream fileStream,
        string fileName,
        string contentType);

    /// <summary>
    /// Get document by path
    /// </summary>
    Task<(bool Succeeded, Stream? FileStream, string? FileName, string? ContentType)> GetDocumentAsync(string filePath);

    /// <summary>
    /// Delete document
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> DeleteDocumentAsync(string filePath);

    /// <summary>
    /// Get all documents for an application
    /// </summary>
    Task<IEnumerable<DocumentInfo>> GetApplicationDocumentsAsync(int applicationId);
}

/// <summary>
/// Document information
/// </summary>
public class DocumentInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
}
