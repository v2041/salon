using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middlewares;

public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title) = exception switch
            {
                BusinessException => (StatusCodes.Status400BadRequest, "Ошибка бизнес-логики"),
                JsonException => (StatusCodes.Status400BadRequest, "Ошибка формата данных"),
                BadHttpRequestException => (StatusCodes.Status400BadRequest, "Некорректный запрос"),
                _ => (StatusCodes.Status500InternalServerError, "Ошибка сервера")
            };
            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Внутренняя ошибка сервера");
            }
            else
            {
                logger.LogInformation("Ошибка запроса: {Message}", exception.Message);
            }
            var problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = exception.Message,
                Instance = context.Request.Path
            };
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json"; 
            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
    }
}