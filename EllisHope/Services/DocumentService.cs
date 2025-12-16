using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Services;

/// <summary>
/// Service for managing document attachments
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".txt" };

    public DocumentService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "applications");

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<(bool Succeeded, string[] Errors, string? FilePath)> UploadDocumentAsync(
        int applicationId,
        string documentType,
        Stream fileStream,
        string fileName,
        string contentType)
    {
        try
        {
            // Validate file
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, new[] { $"File type {extension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}" }, null);
            }

            if (fileStream.Length > _maxFileSize)
            {
                return (false, new[] { $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB" }, null);
            }

            // Generate unique filename
            var uniqueFileName = $"{applicationId}_{documentType}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Save file
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // Update application with document URL
            var application = await _context.ClientApplications.FindAsync(applicationId);
            if (application != null)
            {
                var relativePath = $"/uploads/applications/{uniqueFileName}";
                
                switch (documentType.ToLower())
                {
                    case "medicalclearance":
                        application.MedicalClearanceDocumentUrl = relativePath;
                        break;
                    case "referenceletters":
                        application.ReferenceLettersDocumentUrl = relativePath;
                        break;
                    case "incomeverification":
                        application.IncomeVerificationDocumentUrl = relativePath;
                        break;
                    case "other":
                        application.OtherDocumentsUrl = relativePath;
                        break;
                }

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Document uploaded: {uniqueFileName} for application {applicationId}");
            return (true, Array.Empty<string>(), $"/uploads/applications/{uniqueFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading document for application {applicationId}");
            return (false, new[] { ex.Message }, null);
        }
    }

    public async Task<(bool Succeeded, Stream? FileStream, string? FileName, string? ContentType)> GetDocumentAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fullPath = Path.Combine(_uploadPath, fileName);

            if (!File.Exists(fullPath))
            {
                return (false, null, null, null);
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            return (true, memory, fileName, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving document: {filePath}");
            return (false, null, null, null);
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteDocumentAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fullPath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Remove reference from application
            var application = await _context.ClientApplications
                .Where(a => a.MedicalClearanceDocumentUrl == filePath ||
                           a.ReferenceLettersDocumentUrl == filePath ||
                           a.IncomeVerificationDocumentUrl == filePath ||
                           a.OtherDocumentsUrl == filePath)
                .FirstOrDefaultAsync();

            if (application != null)
            {
                if (application.MedicalClearanceDocumentUrl == filePath)
                    application.MedicalClearanceDocumentUrl = null;
                if (application.ReferenceLettersDocumentUrl == filePath)
                    application.ReferenceLettersDocumentUrl = null;
                if (application.IncomeVerificationDocumentUrl == filePath)
                    application.IncomeVerificationDocumentUrl = null;
                if (application.OtherDocumentsUrl == filePath)
                    application.OtherDocumentsUrl = null;

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Document deleted: {filePath}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting document: {filePath}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<IEnumerable<DocumentInfo>> GetApplicationDocumentsAsync(int applicationId)
    {
        var application = await _context.ClientApplications.FindAsync(applicationId);
        if (application == null)
        {
            return Enumerable.Empty<DocumentInfo>();
        }

        var documents = new List<DocumentInfo>();

        if (!string.IsNullOrEmpty(application.MedicalClearanceDocumentUrl))
        {
            documents.Add(GetDocumentInfo(application.MedicalClearanceDocumentUrl, "Medical Clearance"));
        }

        if (!string.IsNullOrEmpty(application.ReferenceLettersDocumentUrl))
        {
            documents.Add(GetDocumentInfo(application.ReferenceLettersDocumentUrl, "Reference Letters"));
        }

        if (!string.IsNullOrEmpty(application.IncomeVerificationDocumentUrl))
        {
            documents.Add(GetDocumentInfo(application.IncomeVerificationDocumentUrl, "Income Verification"));
        }

        if (!string.IsNullOrEmpty(application.OtherDocumentsUrl))
        {
            documents.Add(GetDocumentInfo(application.OtherDocumentsUrl, "Other Documents"));
        }

        return documents;
    }

    private DocumentInfo GetDocumentInfo(string filePath, string documentType)
    {
        var fileName = Path.GetFileName(filePath);
        var fullPath = Path.Combine(_uploadPath, fileName);
        var fileInfo = new FileInfo(fullPath);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };

        return new DocumentInfo
        {
            FilePath = filePath,
            FileName = fileName,
            DocumentType = documentType,
            ContentType = contentType,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0,
            UploadedDate = fileInfo.Exists ? fileInfo.CreationTimeUtc : DateTime.UtcNow
        };
    }
}
