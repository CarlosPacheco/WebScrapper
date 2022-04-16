using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MisterSpider;
using Serilog;
using SpiderDemo.Model;
using SpiderDemo.Spiders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SpiderDemo
{
    public class Startup
    {
        public static void DependencyInjection(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSingleton(config => Configuration);
            // IoC Logger 
            services.AddSingleton(Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger());
            ConfigOptions config = Configuration.GetSection(ConfigOptions.Position).Get<ConfigOptions>();
            // Add our Config object so it can be injected
            services.Configure<ConfigOptions>(Configuration.GetSection(ConfigOptions.Position));

            services.AddSingleton<INetConnection, NetConnection>();
            services.AddSingleton<ISpiderFactory, SpiderFactory>();

            services.AddSingleton<IWebScrapperManager, WebScrapperManager>();
            services.AddHttpClient(Options.DefaultName, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");
            });
            //    .ConfigurePrimaryHttpMessageHandler(() =>
            //    new HttpClientHandler()
            //    {
            //        // United States proxy, from http://www.hidemyass.com/proxy-list/
            //        // myProxy.BypassProxyOnLocal = true;
            //        Proxy = new WebProxy(config.WebProxyAddress, true),
            //        PreAuthenticate = true,
            //        UseDefaultCredentials = false,
            //        // DefaultProxyCredentials = CredentialCache.DefaultCredentials,
            //    }
            //);

            //services.AddHttpClient(nameof(NetConnectionAirbnb))
            //.ConfigurePrimaryHttpMessageHandler( () => new HttpClientHandler() { CookieContainer = new CookieContainer(), UseCookies = true, });
        }
    }

    internal class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILogger<LifetimeEventsHostedService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        private readonly IWebScrapperManager _webScrapperManager;
        private IServiceProvider _provider { get; }

        public LifetimeEventsHostedService(ILogger<LifetimeEventsHostedService> logger, IWebScrapperManager webScrapperManager, IHostApplicationLifetime appLifetime, IServiceProvider provider)
        {
            _logger = logger;
            _webScrapperManager = webScrapperManager;
            _appLifetime = appLifetime;
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            Company company = new Company
            {
                Symbol = "AMZN",
                Exchange = "NASDAQ",
            };

            //_webScrapperManager.Start<IList<Company>>(typeof(CurrencyfreaksSpider));
            _webScrapperManager.Start<Company>(typeof(RoicSpider), company);
            //_webScrapperManager.Start<Company>(typeof(FinvizSpider), company);
            //_webScrapperManager.Start<Company>(typeof(MorningstarSpider), company, ActivatorUtilities.CreateInstance(_provider, typeof(NetConnectionMorningstar)));

            ISpider<IList<Company>> stocks = _webScrapperManager.Start<IList<Company>>(typeof(DumbstockapiSpider));
            // Perform post-startup activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}
