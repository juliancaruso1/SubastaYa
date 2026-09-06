using System.Net;
using System.Text.Json;
using SubastaYa.Api.DTOs;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Middleware;

// Centraliza la traducción de excepciones de dominio a códigos de estado HTTP,
// evitando devolver 500 genéricos ante errores de negocio esperables.
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
            var (status, mensaje) = ex switch
            {
                RecursoNoEncontradoException => (HttpStatusCode.NotFound, ex.Message),
                SolicitudInvalidaException => (HttpStatusCode.BadRequest, ex.Message),
                SaldoInsuficienteException => (HttpStatusCode.UnprocessableEntity, ex.Message),
                ConflictoConcurrenciaException => (HttpStatusCode.Conflict, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.")
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Error no controlado.");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ErrorResponse(mensaje)));
        }
    }
}
