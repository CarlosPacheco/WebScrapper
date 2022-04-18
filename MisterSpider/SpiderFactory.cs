using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace MisterSpider
{
    public class SpiderFactory : ISpiderFactory
    {
        private IServiceProvider _provider { get; }

        public SpiderFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ISpider<T> GetSpider<T>(Type spiderType, CancellationToken cancellationToken)
        {
            ISpider<T>? spider = (ISpider<T>)ActivatorUtilities.CreateInstance(_provider, spiderType);
            spider.CancellationToken = cancellationToken;
            return spider;
        }

        public ISpider<T> GetSpider<T>(Type spiderType, CancellationToken cancellationToken, params object[] parameters)
        {
            ISpider<T>? spider = (ISpider<T>)ActivatorUtilities.CreateInstance(_provider, spiderType, parameters);
            spider.CancellationToken = cancellationToken;
            return spider;
        }

    }
}