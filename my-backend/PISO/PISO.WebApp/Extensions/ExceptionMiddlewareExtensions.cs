using System.Net;
using PISO.Contracts;
using PISO.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Diagnostics;

namespace PISO.WebApp.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app, ILoggerManager logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature is not null)
                {
                    logger.LogError($"Something went wrong: {contextFeature.Error}");

                    await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Internal Server Error."
                    });
                }
            });
        });
    }
}
