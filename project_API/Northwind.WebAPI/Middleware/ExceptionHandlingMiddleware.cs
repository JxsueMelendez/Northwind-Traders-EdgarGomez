using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Northwind.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception while processing request.");
			await WriteProblemDetailsAsync(context);
		}
	}

	private static Task WriteProblemDetailsAsync(HttpContext context)
	{
		context.Response.ContentType = "application/problem+json";
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

		var problemDetails = new ProblemDetails
		{
			Title = "Unexpected error",
			Detail = "Something went wrong while processing the request.",
			Status = context.Response.StatusCode,
			Instance = context.Request.Path
		};

		return context.Response.WriteAsJsonAsync(problemDetails);
	}
}