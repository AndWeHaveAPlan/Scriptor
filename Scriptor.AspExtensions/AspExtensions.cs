using System;
using System.Collections.Generic;
using System.Linq;
using AndWeHaveAPlan.Scriptor.AspExtensions.Providers;
using AndWeHaveAPlan.Scriptor.Loggers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
                    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                })
                .ConfigureLogging(logBuilder =>
                {
                    var accessor = logBuilder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

                    if (options.OnlyScriptor)
                        logBuilder.ClearProviders();

                    if (options.SetMinimumLogLevel)
                    {
                        // obsolete options.UseMinimumLogLevel
                        logBuilder.SetMinimumLevel(options.MinLogLevel);
                        logBuilder.AddFilter("Microsoft", options.AspMinLogLevel);
                    }

                    logBuilder.AddProvider(new ScriptorLoggerProvider(accessor, options));
                }).Configure(applicationBuilder =>
                {
                    //applicationBuilder.use
                });

            if (options.OnlyScriptor)
                builder.SuppressStatusMessages(true);

            return builder;
        }

        /// <summary>
        /// Add selected headers to ILogger scope (ILogger.BeginParamScope) at the beginning of each request handling
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="headers">TupleValues (string headerName, string scopePropertyName)</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHeadersScope(
            this IApplicationBuilder builder,
            params (string headerName, string scopePropertyName)[] headers)
        {

            if (headers.Length == 0)
            {
                //TODO: get headers pairs from appsettings.json
            }

            // Consume Scriptor-ForwardedScope if present
            builder.Use(async (context, next) =>
            {
                string scopeHeader = context?.Request?.Headers["Scriptor-ForwardedScope"].ToString();
                List<(string, object)> forwardedParams = new List<(string, object)>();
                var logger = context?.RequestServices.GetRequiredService<ILogger<ScriptorLogger>>();

                if (!string.IsNullOrWhiteSpace(scopeHeader))
                {
                    /*foreach (var stringValue in scopeHeader.Value)
                    {
                        var splitIndex = stringValue.IndexOf(':');
                        var key = stringValue.Substring(0, splitIndex);
                        var value = stringValue.Substring(splitIndex);

                        forwardedParams.Add((key, value));
                    }*/

                    var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(scopeHeader);
                    forwardedParams.AddRange(parsed.Select(pair =>
                    {
                        var (key, value) = pair;
                        return (key, value as object);
                    }));
                }

                if (forwardedParams.Any())
                {
                    using (logger.BeginParamScope(forwardedParams.ToArray()))
                    {
                        await next();
                    }
                }
                else
                {
                    await next();
                }
            });

            // Consume Scriptor-ForwardedScope if present
            builder.Use(async (context, next) =>
            {
                var requestHeaders = context?.Request?.Headers;
                List<(string, object)> scopeParams = new List<(string, object)>();
                var logger = context?.RequestServices.GetRequiredService<ILogger<ScriptorLogger>>();

                if (requestHeaders != null)
                {
                    foreach ((string headerName, string scopePropertyName) in headers)
                    {
                        var requestHeader = requestHeaders[headerName];
                        if (requestHeader.Any())
                            scopeParams.Add((scopePropertyName, string.Join(", ", requestHeaders[headerName])));
                    }
                }

                if (scopeParams.Any())
                {
                    using (logger.BeginParamScope(scopeParams.ToArray()))
                    {
                        await next();
                    }
                }
                else
                {
                    await next();
                }
            });

            return builder;
        }
    }
}
