using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scriptor.AspExtensions.Providers;

namespace Scriptor.AspExtensions
{
    public static class AspExtensions
    {
        public static IWebHostBuilder UseScriptor(this IWebHostBuilder builder, bool useJson = false, params KeyValuePair<string, string>[] injectHeaders)
        {
            builder
                .ConfigureLogging(logBuilder =>
                {
                    var accessor = new HttpContextAccessor();

                    logBuilder.ClearProviders();
                    logBuilder.AddProvider(new ScriptorLoggerProvider(accessor, useJson, injectHeaders));
                });

            return builder;
        }
    }
}
