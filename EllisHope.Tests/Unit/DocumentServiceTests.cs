using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Unit;

/// <summary>
/// Unit tests for DocumentService
/// Tests document upload, retrieval, and management
/// </summary>
public class DocumentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _service;
    private readonly string _testUploadPath;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<DocumentService>>();

        // Setup test upload path
        _testUploadPath = Path.Combine(Path.GetTempPath(), "test_uploads", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testUploadPath);

        _environmentMock.Setup(e => e.WebRootPath).Returns(_testUploadPath);

        _service = new DocumentService(_context, _environmentMock.Object, _loggerMock.Object);

        SeedTestData();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();

        // Cleanup test files
        if (Directory.Exists(_testUploadPath))
        {
            Directory.Delete(_testUploadPath, true);
        }
    }

    private void SeedTestData()
    {
        var applicant = new ApplicationUser
        {
            Id = "applicant1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = "applicant1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "555-1234",
            Status = ApplicationStatus.Draft
        };

        _context.Users.Add(applicant);
        _context.ClientApplications.Add(application);
        _context.SaveChanges();
    }

    #region Upload Tests

    [Fact]
    public async Task UploadDocumentAsync_WithValidPdf_UploadsSuccessfully()
    {
        // Arrange
        var fileName = "test.pdf";
        var content = "This is a test PDF file";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var (succeeded, errors, filePath) = await _service.UploadDocumentAsync(
            1,
            "medicalclearance",
            stream,
            fileName,
            "application/pdf");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        Assert.NotNull(filePath);
        Assert.Contains("/uploads/applications/", filePath);
        Assert.EndsWith(".pdf", filePath);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithInvalidExtension_ReturnsError()
    {
        // Arrange
        var fileName = "test.exe";
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var (succeeded, errors, filePath) = await _service.UploadDocumentAsync(
            1,
            "medicalclearance",
            stream,
            fileName,
            "application/octet-stream");

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
        Assert.Null(filePath);
        Assert.Contains("not allowed", errors[0]);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithLargeFile_ReturnsError()
    {
        // Arrange
        var fileName = "test.pdf";
        var largeContent = new byte[11 * 1024 * 1024]; // 11MB (over 10MB limit)
        var stream = new MemoryStream(largeContent);

        // Act
        var (succeeded, errors, filePath) = await _service.UploadDocumentAsync(
            1,
            "medicalclearance",
            stream,
            fileName,
            "application/pdf");

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
        Assert.Null(filePath);
        Assert.Contains("exceeds maximum", errors[0]);
    }

    [Theory]
    [InlineData("test.pdf", "application/pdf")]
    [InlineData("test.doc", "application/msword")]
    [InlineData("test.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("test.jpg", "image/jpeg")]
    [InlineData("test.png", "image/png")]
    [InlineData("test.txt", "text/plain")]
    public async Task UploadDocumentAsync_WithAllowedTypes_Succeeds(string fileName, string contentType)
    {
        // Arrange
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var (succeeded, errors, filePath) = await _service.UploadDocumentAsync(
            1,
            "other",
            stream,
            fileName,
            contentType);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        Assert.NotNull(filePath);
    }

    [Theory]
    [InlineData("medicalclearance", "MedicalClearanceDocumentUrl")]
    [InlineData("referenceletters", "ReferenceLettersDocumentUrl")]
    [InlineData("incomeverification", "IncomeVerificationDocumentUrl")]
    [InlineData("other", "OtherDocumentsUrl")]
    public async Task UploadDocumentAsync_UpdatesApplicationProperty(string documentType, string propertyName)
    {
        // Arrange
        var fileName = "test.pdf";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));

        // Act
        var (succeeded, errors, filePath) = await _service.UploadDocumentAsync(
            1,
            documentType,
            stream,
            fileName,
            "application/pdf");

        // Assert
        Assert.True(succeeded);

        var application = await _context.ClientApplications.FindAsync(1);
        Assert.NotNull(application);

        var property = typeof(ClientApplication).GetProperty(propertyName);
        var value = property?.GetValue(application) as string;
        Assert.NotNull(value);
        Assert.Equal(filePath, value);
    }

    #endregion

    #region Retrieval Tests

    [Fact]
    public async Task GetDocumentAsync_WithExistingFile_ReturnsFile()
    {
        // Arrange - Upload a file first
        var fileName = "test.pdf";
        var content = "Test content";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var (_, _, filePath) = await _service.UploadDocumentAsync(1, "medicalclearance", uploadStream, fileName, "application/pdf");
        Assert.NotNull(filePath);

        // Act
        var (succeeded, fileStream, returnedFileName, contentType) = await _service.GetDocumentAsync(filePath);

        // Assert
        Assert.True(succeeded);
        Assert.NotNull(fileStream);
        Assert.NotNull(returnedFileName);
        Assert.Equal("application/pdf", contentType);

        // Verify content
        using var reader = new StreamReader(fileStream!);
        var retrievedContent = await reader.ReadToEndAsync();
        Assert.Equal(content, retrievedContent);
    }

    [Fact]
    public async Task GetDocumentAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var filePath = "/uploads/applications/nonexistent.pdf";

        // Act
        var (succeeded, fileStream, fileName, contentType) = await _service.GetDocumentAsync(filePath);

        // Assert
        Assert.False(succeeded);
        Assert.Null(fileStream);
        Assert.Null(fileName);
        Assert.Null(contentType);
    }

    #endregion

    #region Deletion Tests

    [Fact]
    public async Task DeleteDocumentAsync_WithExistingFile_DeletesSuccessfully()
    {
        // Arrange - Upload a file first
        var fileName = "test.pdf";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var (_, _, filePath) = await _service.UploadDocumentAsync(1, "medicalclearance", uploadStream, fileName, "application/pdf");
        Assert.NotNull(filePath);

        // Act
        var (succeeded, errors) = await _service.DeleteDocumentAsync(filePath);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        // Verify application property is cleared
        var application = await _context.ClientApplications.FindAsync(1);
        Assert.Null(application?.MedicalClearanceDocumentUrl);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithNonExistentFile_Succeeds()
    {
        // Arrange
        var filePath = "/uploads/applications/nonexistent.pdf";

        // Act
        var (succeeded, errors) = await _service.DeleteDocumentAsync(filePath);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
    }

    #endregion

    #region Document List Tests

    [Fact]
    public async Task GetApplicationDocumentsAsync_WithNoDocuments_ReturnsEmpty()
    {
        // Act
        var documents = await _service.GetApplicationDocumentsAsync(1);

        // Assert
        Assert.Empty(documents);
    }

    [Fact]
    public async Task GetApplicationDocumentsAsync_WithMultipleDocuments_ReturnsAll()
    {
        // Arrange - Upload multiple documents
        await UploadTestDocument(1, "medicalclearance", "medical.pdf");
        await UploadTestDocument(1, "referenceletters", "reference.pdf");
        await UploadTestDocument(1, "incomeverification", "income.pdf");

        // Act
        var documents = await _service.GetApplicationDocumentsAsync(1);

        // Assert
        Assert.Equal(3, documents.Count());
        Assert.Contains(documents, d => d.DocumentType == "Medical Clearance");
        Assert.Contains(documents, d => d.DocumentType == "Reference Letters");
        Assert.Contains(documents, d => d.DocumentType == "Income Verification");
    }

    [Fact]
    public async Task GetApplicationDocumentsAsync_WithNonExistentApplication_ReturnsEmpty()
    {
        // Act
        var documents = await _service.GetApplicationDocumentsAsync(99999);

        // Assert
        Assert.Empty(documents);
    }

    #endregion

    #region Helper Methods

    private async Task<string?> UploadTestDocument(int applicationId, string documentType, string fileName)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test content"));
        var (_, _, filePath) = await _service.UploadDocumentAsync(
            applicationId,
            documentType,
            stream,
            fileName,
            "application/pdf");
        return filePath;
    }

    #endregion
}
