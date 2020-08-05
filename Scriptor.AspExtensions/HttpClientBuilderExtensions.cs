using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AndWeHaveAPlan.Scriptor.AspExtensions.Providers;
using AndWeHaveAPlan.Scriptor.Loggers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public static class HttpClientBuilderExtensions
    {
        //private static IExternalScopeProvider _defaultScopeProvider = new LoggerExternalScopeProvider();

        public static IHttpClientBuilder AddTracingHeadersForwarding(this IHttpClientBuilder builder, params string[] headers)
        {
            builder.Services.TryAddSingleton<IHttpContextAccessor>();

            builder.ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                foreach (var headerName in headers)
                {
                    var headerValue = httpContext.Request
                        .Headers[headerName];

                    httpClient.DefaultRequestHeaders.Add(headerName, headerValue.ToArray());
                }
            });

            return builder;
        }

        public static IHttpClientBuilder AddScriptorScopeForwarding(this IHttpClientBuilder builder, IExternalScopeProvider scopeProvider = null)
        {
            builder.AddHttpMessageHandler(provider => new HeaderInjectHandler(scopeProvider ?? ScriptorLoggerProvider.DefaultScopeProvider));

            return builder;
        }
    }

    public class HeaderInjectHandler : DelegatingHandler
    {
        private readonly IExternalScopeProvider _scopeProvider;

        public HeaderInjectHandler(IExternalScopeProvider scopeProvider) : base()
        {
            _scopeProvider = scopeProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var scopeParams = ScriptorLogger.ExtractFieldFromScope(_scopeProvider);
            request.Headers.Add("Scriptor-ForwardedScope", JsonConvert.SerializeObject(scopeParams));

            return base.SendAsync(request, cancellationToken);
        }
    }
}
