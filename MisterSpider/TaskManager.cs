﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MisterSpider
{
    public class TaskManager : IParallelManager
    {
        private ConcurrentBag<Tuple<Action<Url>, Url>> _queueItensToProcess;

        private int _itensProcessDoneCount;

        public int ItensToProcessCount { get { return _itensProcessDoneCount; } }

        public bool IsCompleted => throw new NotImplementedException();

        public CancellationToken CancellationToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private ConfigOptions _config;

        public TaskManager(IOptions<ConfigOptions> config)
        {
            _config = config.Value;
            _queueItensToProcess = new ConcurrentBag<Tuple<Action<Url>, Url>>();
        }

        public void Add(Action<Url> fetchNewPage, Url url)
        {
            _queueItensToProcess.Add(Tuple.Create(fetchNewPage, url));
        }

        public void StartWait()
        {
            while (_queueItensToProcess.Any())
            {
                //copy the list to a local list and clean the list
                List<Tuple<Action<Url>, Url>> itens = new List<Tuple<Action<Url>, Url>>(_queueItensToProcess);
                _queueItensToProcess = new ConcurrentBag<Tuple<Action<Url>, Url>>();

                Interlocked.Add(ref _itensProcessDoneCount, itens.Count);
                Parallel.ForEach(itens, new ParallelOptions { MaxDegreeOfParallelism = _config.MaximumThreads },
                (currentItem, loop, j) =>
                {
                    if (_config.ShouldSleep) Thread.Sleep(IdleTime());
                    currentItem.Item1(currentItem.Item2);
                    Interlocked.Decrement(ref _itensProcessDoneCount);
                });
            }
        }

        public int IdleTime()
        {
            return new Random().Next(_config.MinThreadIdleTime, _config.MaxThreadIdleTime + 1);
        }

        public void IncrementItensProcess()
        {
        }

        public void Retry(Action<Url> fetchNewPage, Url url)
        {
            Add(fetchNewPage, url);
        }
    }
}
