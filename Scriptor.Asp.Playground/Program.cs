using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scriptor.AspExtensions;

namespace Scriptor.Asp.Playground
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .SuppressStatusMessages(true)
                .UseStartup<Startup>()
                .UseScriptor(
                    Environment.GetEnvironmentVariable("LOG_FORMAT")?.ToLower() == "json",
                    new KeyValuePair<string, string>("request_idf", "X-Request-Id"),
                    new KeyValuePair<string, string>("cf_ray_id", "CF-Ray")
                )
                .UseUrls("http://+:3000");
    }
}
