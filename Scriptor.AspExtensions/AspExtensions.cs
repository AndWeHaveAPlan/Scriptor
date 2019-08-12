using System.Collections.Generic;
using System.Linq;
using AndWeHaveAPlan.Scriptor.AspExtensions.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public static class AspExtensions
    {
        public static IWebHostBuilder UseScriptor(this IWebHostBuilder builder, bool useJson = false, params KeyValuePair<string, string>[] injectHeaders)
        {
            builder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                    services.Configure<ApiBehaviorOptions>(opts =>
                    {
                        opts.InvalidModelStateResponseFactory = context =>
                        {
                            var details = new ValidationProblemDetails(context.ModelState)
                            {
                                Extensions =
                                {
                                    {"request_d", context.HttpContext?.Request.Headers["X-Request-Id"].FirstOrDefault()},
                                    {"cf_ray_id", context.HttpContext?.Request.Headers["CF-RAY"].FirstOrDefault()},
                                    {"asp_trace", context.HttpContext?.TraceIdentifier}
                                }
                            };

                            return new BadRequestObjectResult(details)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        };
                    });
                })
                .ConfigureLogging(logBuilder =>
                {
                    var accessor = logBuilder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

                    logBuilder.ClearProviders();
                    logBuilder.AddProvider(new ScriptorLoggerProvider(accessor, useJson, injectHeaders));
                });

            return builder;
        }
    }
}
