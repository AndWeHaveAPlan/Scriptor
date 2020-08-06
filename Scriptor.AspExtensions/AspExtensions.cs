using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public static IApplicationBuilder UseHeadersAsScope(
            this IApplicationBuilder builder,
            params (string headerName, string scopePropertyName)[] headers)
        {

            if (headers.Length == 0)
            {
                //TODO: get headers pairs from appsettings.json
            }

            // Consume Scriptor-ForwardedScope if present
            builder.Use(ScopeForwardedHeaders);

            // Consume selected headers
            builder.Use(async (context, next) =>
            {
                await ScopeHeaders(context, next, headers);
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private static async Task ScopeForwardedHeaders(HttpContext context, Func<Task> next)
        {
            string scopeHeader = context?.Request?.Headers["Scriptor-ForwardedScope"].ToString();
            List<(string, object)> forwardedParams = new List<(string, object)>();


            if (!string.IsNullOrWhiteSpace(scopeHeader))
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(scopeHeader);
                forwardedParams.AddRange(parsed.Select(pair =>
                {
                    var (key, value) = pair;
                    return (key, value as object);
                }));
            }

            await CreateScope(context, next, forwardedParams);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static async Task ScopeHeaders(HttpContext context, Func<Task> next, (string headerName, string scopePropertyName)[] headers)
        {
            var requestHeaders = context?.Request?.Headers;
            List<(string, object)> scopeParams = new List<(string, object)>();


            if (requestHeaders != null)
            {
                foreach ((string headerName, string scopePropertyName) in headers)
                {
                    var requestHeader = requestHeaders[headerName];
                    if (requestHeader.Any())
                        scopeParams.Add((scopePropertyName, string.Join(", ", requestHeaders[headerName])));
                }
            }

            await CreateScope(context, next, scopeParams);
        }

        /// <summary>
        /// Continue pipeline with scope if scopeParams present
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="scopeParams"></param>
        /// <returns></returns>
        private static async Task CreateScope(HttpContext context, Func<Task> next, List<(string, object)> scopeParams)
        {
            if (scopeParams.Any())
            {
                var logger = context?.RequestServices.GetRequiredService<ILogger<ScriptorLogger>>();
                using (logger.BeginParamScope(scopeParams.ToArray()))
                {
                    await next();
                }
            }
            else
            {
                await next();
            }
        }
    }
}
