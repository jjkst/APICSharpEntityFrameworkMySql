using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RukuServiceApi.Models;
using RukuServiceApi.Services;

namespace RukuServiceApi.UnitTests.Services;

[TestClass]
public sealed class FileUploadServiceTests
{
    private Mock<ILogger<FileUploadService>> _loggerMock = null!;

    private FileUploadService CreateService(FileUploadSettings? settings = null)
    {
        _loggerMock = new Mock<ILogger<FileUploadService>>();
        var options = Options.Create(settings ?? new FileUploadSettings());
        return new FileUploadService(options, _loggerMock.Object);
    }

    private static Mock<IFormFile> CreateMockFile(
        string fileName = "test.jpg",
        string contentType = "image/jpeg",
        long length = 1024)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        return fileMock;
    }

    [TestMethod]
    public void ValidateFile_WithValidFile_ShouldReturnTrue()
    {
        var service = CreateService();
        var file = CreateMockFile().Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }

    [TestMethod]
    public void ValidateFile_WithOversizedFile_ShouldReturnFalse()
    {
        var service = CreateService(new FileUploadSettings { MaxFileSizeBytes = 1000 });
        var file = CreateMockFile(length: 2000).Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("exceeds maximum");
    }

    [TestMethod]
    public void ValidateFile_WithInvalidExtension_ShouldReturnFalse()
    {
        var service = CreateService();
        var file = CreateMockFile(fileName: "malware.exe", contentType: "application/octet-stream").Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("not allowed");
    }

    [TestMethod]
    public void ValidateFile_WithInvalidMimeType_ShouldReturnFalse()
    {
        var service = CreateService();
        var file = CreateMockFile(fileName: "test.jpg", contentType: "text/html").Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("MIME type");
    }

    [TestMethod]
    public void ValidateFile_WithZeroLengthFile_ShouldReturnFalse()
    {
        var service = CreateService();
        var file = CreateMockFile(length: 0).Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("No file provided");
    }

    [TestMethod]
    public void ValidateFile_WithSuspiciousFilename_ShouldReturnFalse()
    {
        var service = CreateService();
        var file = CreateMockFile(fileName: "../etc/passwd.jpg").Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("suspicious");
    }

    [TestMethod]
    public void ValidateFile_WithScriptInFilename_ShouldReturnFalse()
    {
        var service = CreateService();
        var file = CreateMockFile(fileName: "javascript_test.jpg").Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeFalse();
        errorMessage.Should().Contain("suspicious");
    }

    [TestMethod]
    public void GenerateSecureFileName_ShouldPreserveExtension()
    {
        var service = CreateService();

        var result = service.GenerateSecureFileName("photo.png");

        result.Should().EndWith(".png");
    }

    [TestMethod]
    public void GenerateSecureFileName_ShouldNotContainOriginalName()
    {
        var service = CreateService();

        var result = service.GenerateSecureFileName("my-original-photo.jpg");

        result.Should().NotContain("my-original-photo");
    }

    [TestMethod]
    public void GenerateSecureFileName_ShouldGenerateUniqueNames()
    {
        var service = CreateService();

        var name1 = service.GenerateSecureFileName("photo.jpg");
        var name2 = service.GenerateSecureFileName("photo.jpg");

        name1.Should().NotBe(name2);
    }

    [TestMethod]
    public void ValidateFile_WithAllowedPdfFile_ShouldReturnTrue()
    {
        var service = CreateService();
        var file = CreateMockFile(
            fileName: "document.pdf",
            contentType: "application/pdf",
            length: 5000).Object;

        var result = service.ValidateFile(file, out var errorMessage);

        result.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }
}
