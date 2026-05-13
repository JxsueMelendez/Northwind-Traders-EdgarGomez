using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Northwind.WebAPI.Middleware;

namespace Northwind.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldPassThrough_WhenNoException()
    {
        var logger = new LoggerFactory().CreateLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            await context.Response.WriteAsync(string.Empty);
        }, logger);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnProblemDetails_WhenExceptionThrown()
    {
        var logger = new LoggerFactory().CreateLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("Boom"), logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/orders";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        Assert.Equal("Unexpected error", payload.GetProperty("title").GetString());
        Assert.Equal("Boom", payload.GetProperty("detail").GetString());
        Assert.Equal("/api/orders", payload.GetProperty("instance").GetString());
        Assert.True(payload.TryGetProperty("exception", out _));
    }
}
