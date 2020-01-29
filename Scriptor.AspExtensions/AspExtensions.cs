using System;
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


                    //                    logBuilder.AddFilter("Default", options.MinLogLevel);
                    logBuilder.SetMinimumLevel(options.MinLogLevel);
                    logBuilder.AddFilter("Microsoft", options.AspMinLogLevel);

                    logBuilder.AddProvider(new ScriptorLoggerProvider(accessor, options));
                });

            if (options.OnlyScriptor)
                builder.SuppressStatusMessages(true);

            return builder;
        }
    }
}
