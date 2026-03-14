using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RukuServiceApi.Middleware;
using RukuServiceApi.Models;

namespace RukuServiceApi.UnitTests.Middleware;

[TestClass]
public sealed class GlobalExceptionMiddlewareTests
{
    private Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock = null!;
    private Mock<IWebHostEnvironment> _envMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");
    }

    private GlobalExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new GlobalExceptionMiddleware(next, _loggerMock.Object, _envMock.Object);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ErrorResponse?> ReadErrorResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    [TestMethod]
    public async Task InvokeAsync_NoException_ShouldCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_UnauthorizedAccessException_ShouldReturn401()
    {
        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Unauthorized access");
    }

    [TestMethod]
    public async Task InvokeAsync_ArgumentException_ShouldReturn400()
    {
        var middleware = CreateMiddleware(_ => throw new ArgumentException("Bad argument"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Bad argument");
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidOperationException_ShouldReturn400()
    {
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Invalid op"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Invalid op");
    }

    [TestMethod]
    public async Task InvokeAsync_FileNotFoundException_ShouldReturn404()
    {
        var middleware = CreateMiddleware(_ => throw new FileNotFoundException());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Resource not found");
    }

    [TestMethod]
    public async Task InvokeAsync_TimeoutException_ShouldReturn408()
    {
        var middleware = CreateMiddleware(_ => throw new TimeoutException());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(408);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Request timeout");
    }

    [TestMethod]
    public async Task InvokeAsync_GenericException_ShouldReturn500()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("Something broke"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("An error occurred while processing your request");
    }

    [TestMethod]
    public async Task InvokeAsync_GenericExceptionInDevelopment_ShouldIncludeDetails()
    {
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
        var middleware = CreateMiddleware(_ => throw new Exception("Detailed error"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        var error = await ReadErrorResponse(context);
        error!.Message.Should().Be("Detailed error");
        error.Details.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task InvokeAsync_Exception_ShouldSetJsonContentType()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("error"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/json");
    }
}
