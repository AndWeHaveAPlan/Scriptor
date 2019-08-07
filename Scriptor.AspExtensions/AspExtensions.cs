using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scriptor.AspExtensions.Providers;

namespace Scriptor.AspExtensions
{
    public static class AspExtensions
    {
        public static IWebHostBuilder UseScriptor(this IWebHostBuilder builder, bool useJson = false, params KeyValuePair<string, string>[] injectHeaders)
        {
            builder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
