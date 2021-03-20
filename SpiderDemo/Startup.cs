using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MisterSpider;
using MisterSpider.Configurations;
using Serilog;
using System;
using System.Collections.Generic;
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
            //ConfigOptions config = Configuration.GetSection(ConfigOptions.Position).Get<ConfigOptions>();

            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure<ConfigOptions>(Configuration.GetSection(ConfigOptions.Position));

            services.AddSingleton<INetConnection, NetConnection>();
            services.AddSingleton<ISpiderFactory, SpiderFactory>();

            services.AddSingleton<IWebScrapperManager, WebScrapperManager>();

        }
    }

    internal class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILogger<LifetimeEventsHostedService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        private readonly IWebScrapperManager _webScrapperManager;

        public LifetimeEventsHostedService(ILogger<LifetimeEventsHostedService> logger, IWebScrapperManager webScrapperManager, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _webScrapperManager = webScrapperManager;
            _appLifetime = appLifetime;
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
            _webScrapperManager.Start<object>(new Dictionary<string, string>
                {
                      {"MisterSpider.Spiders.BookingSpider", "http://www.booking.com/hotel/pt/lisb-on-hostel.html"}
                }, new SpiderParams
                {
                    CheckIn = Convert.ToDateTime("2021-08-03"),
                    CheckOut = Convert.ToDateTime("2021-08-04"),
                    Adults = "1",
                    Children = "0",
                    Nights = (Convert.ToDateTime("2021-08-04") - Convert.ToDateTime("2021-08-03")).Days.ToString(),
                });
            // Perform post-startup activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}
