using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;

namespace MisterSpider
{
    public static class DependencyInjection
    {
        public static void AddMisterSpider(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigOptions config = configuration.GetSection(ConfigOptions.Position).Get<ConfigOptions>();

            if (string.IsNullOrWhiteSpace(config.LogFolder))
            {
                throw new Exception("Are you missing the appsettings section Spider?");
            }

            // Add our Config object so it can be injected
            services.Configure<ConfigOptions>(configuration.GetSection(ConfigOptions.Position));

            services.AddSingleton<INetConnection, NetConnection>();
            services.AddTransient<IParallelManager, ThreadManager>();
            services.AddSingleton<ISpiderFactory, SpiderFactory>();
            services.AddScoped<IWebScrapperManager, WebScrapperManager>();
            services.AddHttpClient(Options.DefaultName, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");
            });
        }
    }
}
