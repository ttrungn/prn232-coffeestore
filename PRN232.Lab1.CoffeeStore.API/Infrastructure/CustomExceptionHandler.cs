using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.API.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(InvalidOperationException), HandleInvalidOperationException },
            { typeof(ArgumentException), HandleArgumentException },
            { typeof(ArgumentNullException), HandleArgumentException }
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var handler = _exceptionHandlers
            .FirstOrDefault(h => h.Key.IsAssignableFrom(exceptionType)).Value;

        if (handler is null)
        {
            await HandleInternalServerError(httpContext, exception);
            return true;
        }
        
        await handler.Invoke(httpContext, exception);
        return true;
    }
    
    private async Task HandleInvalidOperationException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse()
        {
            Success = false,
            Message = ex.Message
        });
    }

    private async Task HandleArgumentException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse()
        {
            Success = false,
            Message = ex.Message
        });
    }
    
    private async Task HandleInternalServerError(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse()
        {
            Success = false,
            Message = "Internal server error occurred."
        });
    }
}
