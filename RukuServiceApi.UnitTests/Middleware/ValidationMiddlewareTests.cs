using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RukuServiceApi.Middleware;

namespace RukuServiceApi.UnitTests.Middleware;

[TestClass]
public sealed class ValidationMiddlewareTests
{
    private Mock<ILogger<ValidationMiddleware>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ValidationMiddleware>>();
    }

    private ValidationMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ValidationMiddleware(next, _loggerMock.Object);
    }

    private static DefaultHttpContext CreatePostContext(string body, string contentType = "application/json")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();
        return context;
    }

    [TestMethod]
    public async Task InvokeAsync_ValidJson_ShouldCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreatePostContext("{\"name\": \"test\"}");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidJson_ShouldReturn400()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreatePostContext("{invalid json}");

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidJson_ShouldReturnErrorMessage()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreatePostContext("{invalid}");

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("Invalid JSON format");
    }

    [TestMethod]
    public async Task InvokeAsync_GetRequest_ShouldSkipValidation()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_PutWithInvalidJson_ShouldReturn400()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreatePostContext("{bad json}");
        context.Request.Method = "PUT";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public async Task InvokeAsync_MultipartFormData_ShouldSkipValidation()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreatePostContext("not json", "multipart/form-data; boundary=----");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_FormUrlEncoded_ShouldSkipValidation()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreatePostContext("key=value", "application/x-www-form-urlencoded");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_EmptyBody_ShouldCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreatePostContext("");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_DeleteRequest_ShouldSkipValidation()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Method = "DELETE";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
