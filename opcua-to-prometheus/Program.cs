using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using opcua_to_prometheus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opcua_to_prometheus
{
    public class Program
    {

        public async static Task Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();

            var plcService = webHost.Services.GetRequiredService<PLCService>();
            await plcService.InitializeAsync();

            webHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseStartup<Startup>()
                    .UseUrls("http://0.0.0.0:5000");
                });
    }
}
