using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RukuServiceApi.Middleware;

namespace RukuServiceApi.UnitTests.Middleware;

[TestClass]
public sealed class SecurityHeadersMiddlewareTests
{
    [TestMethod]
    public async Task InvokeAsync_ShouldAddXContentTypeOptions()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldAddXFrameOptions()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldAddXXssProtection()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldAddReferrerPolicy()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [TestMethod]
    public async Task InvokeAsync_HttpsRequest_ShouldAddHstsHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["Strict-Transport-Security"].ToString()
            .Should().Be("max-age=31536000; includeSubDomains");
    }

    [TestMethod]
    public async Task InvokeAsync_HttpRequest_ShouldNotAddHstsHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers.ContainsKey("Strict-Transport-Security").Should().BeFalse();
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        var nextCalled = false;
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
