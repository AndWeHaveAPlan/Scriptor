using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public static class HttpClientBuilderExtensions
    {
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
    }
}
