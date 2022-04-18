using System;
using System.Collections.Generic;
using System.Threading;

namespace MisterSpider
{
    public interface IWebScrapperManager : IDisposable
    {
        /// <summary>
        /// Concurrent methos, each spider will run inside a thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classTypes"></param>
        /// <returns></returns>
        IList<T> StartConcurrent<T>(List<string> classTypes);

        T? StartSingle<T>(string classType, params object[] parameters);

        T? StartSingle<T>(Type classType, params object[] parameters);

        IList<T> StartSingleList<T>(Type classType, params object[] parameters);

        /// <summary>
        /// Concurrent methos, each spider will run inside a thread
        /// </summary>
        /// <param name="classTypes"></param>
        /// <param name="sleepTime"></param>
        void StartConcurrent(List<string> classTypes, TimeSpan sleepTime);

        IList<SpiderConfiguration<T>> Start<T>(IList<SpiderConfiguration<T>> spiderConfigs);

        CancellationToken CancellationToken { get; set; }
    }
}
