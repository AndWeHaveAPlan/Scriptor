using System;
using System.Collections.Generic;
using System.Linq;
using AndWeHaveAPlan.Scriptor.AspExtensions.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public static class AspExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="useJson"></param>
        /// <param name="injectHeaders"></param>
        /// <returns></returns>
        [Obsolete]
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
                    logBuilder.AddProvider(
                        new ScriptorLoggerProvider(accessor, useJson, injectHeaders.Select(ih => (ih.Key, ih.Value)).ToArray())
                        );
                });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseScriptor(this IWebHostBuilder builder, Action<ScriptorOptions> optionsAction)
        {
            var options = new ScriptorOptions();

            optionsAction?.Invoke(options);

            builder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                })
                .ConfigureLogging(logBuilder =>
                {
                    var accessor = logBuilder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

                    if (options.OnlyScriptor)
                        logBuilder.ClearProviders();
                    logBuilder.AddProvider(new ScriptorLoggerProvider(accessor, options.Json, options.InjectedHeaders));
                });

            if (options.OnlyScriptor)
                builder.SuppressStatusMessages(true);

            return builder;
        }
    }
}
