using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace MisterSpider
{
    public class SpiderFactory : ISpiderFactory
    {
        private IServiceProvider _provider { get; }

        public SpiderFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ISpider<T> GetSpider<T>(Type spiderType)
        {
            return (ISpider<T>)ActivatorUtilities.CreateInstance(_provider, spiderType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding);
        }
    }
}