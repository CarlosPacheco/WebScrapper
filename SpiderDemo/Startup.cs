using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MisterSpider;
using Serilog;
using SpiderDemo.Model;
using SpiderDemo.Spiders;
using System;
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

            services.AddMisterSpider(Configuration);

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
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private IServiceProvider _provider { get; }

        public LifetimeEventsHostedService(ILogger<LifetimeEventsHostedService> logger, IHostApplicationLifetime appLifetime, IServiceProvider provider)
        {
            _logger = logger;      
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

        Thread thread;
        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");

            thread = new Thread(new ThreadStart(RunApp));
            thread.Start();
        }

        private void RunApp()
        {
            using IServiceScope? scope = _provider.CreateScope();
            using IWebScrapperManager _webScrapperManager = scope.ServiceProvider.GetService<IWebScrapperManager>()!;
            _webScrapperManager.CancellationToken = _cancellationTokenSource.Token;
            for (int i = 0; i < 25; i++)
            {
                //if (_cancellationTokenSource.Token.IsCancellationRequested) _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                Company company = new Company
                {
                    Symbol = "AMZN",
                    Exchange = "NASDAQ",
                };

                //_webScrapperManager.Start<IList<Company>>(typeof(CurrencyfreaksSpider));

                _webScrapperManager.StartSingle<Company>(typeof(FinvizSpider), company);
                _webScrapperManager.StartSingle<Company>(typeof(MorningstarSpider), company, ActivatorUtilities.CreateInstance(scope.ServiceProvider, typeof(NetConnectionMorningstar)));
                _webScrapperManager.StartSingle<Company>(typeof(RoicSpider), company);

                // ISpider<IList<Company>> stocks = _webScrapperManager.Start<IList<Company>>(typeof(DumbstockapiSpider));
                // Perform post-startup activities here
            }

        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
            // Perform post-stopped activities here
            if (thread.IsAlive)
            {
                _cancellationTokenSource.Cancel();
                thread.Join();
            }
            _cancellationTokenSource.Dispose();
        }
    }

}
