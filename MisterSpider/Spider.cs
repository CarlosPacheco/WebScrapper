using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MisterSpider
{
    public abstract class Spider<T> : ISpider<T>
    {
        protected ILogger _logger { get; }

        protected INetConnection Connection { get; set; }

        protected IParallelManager ParallelManager { get; set; }

        private ConfigOptions _config;

        public ConcurrentBag<T> ExtractData { get; } = new ConcurrentBag<T>();

        protected ConcurrentBag<string> UrlsSeen { get; set; } = new ConcurrentBag<string>();

        public object Tag { get; set; }

        protected virtual bool UseSitemap { get { return false; } }

        /// <summary>
        /// Seed URLs go here. These are the root URLs the crawler will visit first. They will always be 
        ///visited even if the domain is not on the whitelist or is an excluded domain. The *entire* URL
        ///is required and should be formatted like this:
        /// http://www.google.com
        /// http://www.facebook.com 
        /// </summary>
        protected List<string> Urls { get; set; } = new List<string>();

        protected List<string> SitemapURLs { get; set; } = new List<string>();

        /// <summary>
        /// Save the itens inside a JSON file
        /// </summary>
        protected FileJson FileManager { get; set; }

        /// <summary>
        /// Logs the Fails Urls when Crawl a page
        /// </summary>
        protected FileJson FileUrlsLog { get; set; }
       
        protected Spider(ILogger<Spider<T>> logger, INetConnection connection, IOptions<ConfigOptions> config)
        {
            _config = config.Value;
            _logger = logger;
            Connection = connection;

            ParallelManager = new ThreadManager(config);

            FileManager = new FileJson($"{GetType().Name}{"Itens.json"}", _config.LogFolder);
            FileUrlsLog = new FileJson($"{GetType().Name}{"ErrorItens.json"}", _config.LogFolder);
        }

        protected abstract T Crawl(Page page);

        #region AddProcess
        /// <summary>
        /// Enqueue a new Url to the Urls Queue
        /// Add to the UrlsSeen list 
        /// </summary>
        /// <returns></returns>
        protected bool AddProcess(Url url)
        {
            string link = url.uri.AbsoluteUri.ToLower();

            if (UrlsSeen.Contains(link))
                _logger.LogInformation("Skipping...URL already queued", link);
            else if (_config.UseWhiteList && !IsWhiteListedDomain(url.uri.Authority))
                _logger.LogInformation("URL domain not on whitelist", link);
            else if (IsExcludedDomain(link))
                _logger.LogInformation("Skipping...URL domain is excluded", link);
            else if (IsExcludedFileType(link))
                _logger.LogInformation("Skipping...file type is excluded", link);
            else if (ShouldDownload(link))
            {
                UrlsSeen.Add(link);
                Download(url.uri);
            }
            else if (ShouldContinue(url.depth))
            {
                UrlsSeen.Add(link);
                ParallelManager.AddProcess(FetchNewPage, url);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enqueue a new Url to the Urls Queue
        /// Add to the UrlsSeen list 
        /// depth = -1 (root)
        /// </summary>
        /// <param name="urllink">No clean Url</param>
        /// <returns></returns>
        protected bool AddProcess(string urllink)
        {
            return AddProcess(new Url(new Uri(urllink), -1));
        }

        /// <summary>
        /// Enqueue a news Urls to the Urls Queue
        /// Add to the UrlsSeen list 
        /// </summary>
        /// <param name="urllinks">No clean Urls List</param>
        /// <returns></returns>
        protected bool AddProcess(List<string> urllinks)
        {
            bool ret = true;
            foreach (var item in urllinks)
            {
                ret = AddProcess(new Url(new Uri(item), -1));
            }

            return ret;
        }

        /// <summary>
        /// Enqueue a new Url to the Urls Queue
        /// Add to the UrlsSeen list 
        /// </summary>
        /// <param name="urllink">No clean Url</param>
        /// <param name="depth"></param>
        /// <returns></returns>
        protected bool AddProcess(string urllink, int depth)
        {
            return AddProcess(new Url(new Uri(urllink), depth));
        }

        /// <summary>
        /// Enqueue a news Urls to the Urls Queue
        /// Add to the UrlsSeen list 
        /// </summary>
        /// <param name="urllinks">No clean Urls List</param>
        /// <param name="depth"></param>
        /// <returns></returns>
        protected bool AddProcess(List<string> urllinks, int depth)
        {
            bool ret = true;
            foreach (var item in urllinks)
            {
                ret = AddProcess(new Url(new Uri(item), depth));
            }

            return ret;
        }

        /// <summary>
        /// Enqueue a new Url to the Urls Queue
        /// Add to the UrlsSeen list 
        /// Clean the url and Page depth +1
        /// </summary>
        /// <param name="urllink">No clean Url</param>
        /// <param name="page"></param>
        /// <returns></returns>
        protected bool AddProcess(string urllink, Page page)
        {
            return AddProcess(new Url(new Uri(page.CleanUrl(urllink)), page.Url.depth + 1));
        }

        /// <summary>
        /// Enqueue a news Urls to the Urls Queue
        /// Add to the UrlsSeen list
        /// Clean each url and Page depth +1
        /// </summary>
        /// <param name="urllinks">No clean Urls List</param>
        /// <param name="page"></param>
        /// <returns></returns>
        protected bool AddProcess(List<string> urllinks, Page page)
        {
            bool ret = true;
            var depth = page.Url.depth;
            foreach (var item in urllinks)
            {
                ret = AddProcess(new Url(new Uri(page.CleanUrl(item)), depth + 1));
            }

            return ret;
        }

        #endregion AddProcess

        protected void DownloadFile(string urllink)
        {
            Url url = new Url(new Uri(urllink), -1);
            UrlsSeen.Add(urllink);
            Download(url.uri);
        }

        protected void DownloadFile(string urllink, Page page)
        {
            Url url = new Url(new Uri(urllink), page.Url.depth);
            UrlsSeen.Add(urllink);
            Download(url.uri);
        }

        public void Go()
        {
            List<string> urls = UseSitemap ? GetUrlsFromSitemap(SitemapURLs) : Urls;

            foreach (string seed in urls)
            {
                AddProcess(seed);
            }

            if (urls.Count == 0)
            {
                Console.WriteLine("Need at least one seed URL.");
            }

            //wait for all threads
            ParallelManager.StartWait();
        }

        public void Go(object tag)
        {
            Tag = tag;
            Go();
        }

        protected void FetchNewPage(Url url)
        {
            string link = url.uri.AbsoluteUri;
            Page page = new Page(_logger, url, Connection.Go(url));

            if (!string.IsNullOrEmpty(page.Source))
            {
                _logger.LogInformation("Page load successful, {0}", link);
                T item = default;

                try
                {
                    item = Crawl(page);
                    _logger.LogInformation("Finished crawling page. {0}", GetUrlState());
                }
                catch (Exception ex)//parser error
                {
                    _logger.LogDebug("Parser Error on this url, {0}", ex.Message);
                    if (_config.SaveErrorItens)
                    {
                        FileUrlsLog.Write(page.Url);
                    }
                }
                finally
                {
                    if (item != null)
                    {
                        // FileManager.Write(item);
                        ExtractData.Add(item);
                    }
                    ParallelManager.IncrementItensProcess();
                }
            }
            else//if the page.source is null or empty this mean connection error
            {
                if (_config.TryAgainOnError)
                {
                    //put the url back to the queue
                    ParallelManager.AddThreadPool(FetchNewPage, url);
                    _logger.LogInformation("Queuing again this url, {0}", link);
                }
                else
                {
                    ParallelManager.IncrementItensProcess();
                }
            }
        }

        public string GetUrlState()
        {
            return $"{ParallelManager.ItensToProcessCount}/{UrlsSeen.Count}";
        }

        public List<string> GetUrlsFromSitemap(List<string> list)
        {
            int errorCount = 0;

            var urls = new List<string>();

            Queue<string> URLQueue = new Queue<string>(list);

            while (URLQueue.Any())
            {
                //dequeue and process the item
                var url = URLQueue.Dequeue();
                XmlDocument xml = new XmlDocument();

                if (url.ToLower().EndsWith(".gz"))
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Timeout = 1000 * 60 * 60; // milliseconds  
                    try
                    {
                        using (Stream responseStream = req.GetResponse().GetResponseStream())
                        {
                            GZipStream zip = new GZipStream(responseStream, CompressionMode.Decompress);
                            xml.Load(zip);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("GetURLsFromSitemap", ex);
                        errorCount++;
                        if (errorCount > 6) return urls;//try only 6 times
                        URLQueue.Enqueue(url);
                        continue;
                    }

                }
                else
                {
                    xml.Load(url);
                }

                XmlNamespaceManager manager = new XmlNamespaceManager(xml.NameTable);
                manager.AddNamespace("s", xml.DocumentElement.NamespaceURI); //Using xml's properties instead of hard-coded URI  
                XmlNodeList xnList = xml.SelectNodes("/s:urlset/s:url/s:loc", manager);
                var parallelLoop1 = xnList.Count;

                //process the file rows
                Parallel.ForEach(Partitioner.Create(0, xnList.Count), () => new List<string>(),
                (range, loop, j, prevlocal) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        prevlocal.Add(xnList[i].InnerText);
                    }
                    return prevlocal;
                }, (local) =>
                {
                    foreach (var item in local)
                    {
                        if (item.ToLower().EndsWith(".htm") || item.ToLower().EndsWith(".html"))
                        {
                            lock (urls) urls.Add(item);
                        }
                        else
                        {
                            lock (URLQueue) URLQueue.Enqueue(item);
                        }
                    }
                });

            }

            return urls;
        }

        public bool ShouldContinue(int currentDepth)
        {
            return currentDepth < _config.MaximumDepth;
        }

        public bool IsExcludedDomain(string url)
        {
            bool isExcluded = false;

            try
            {
                Uri uri = new Uri(url.ToLower());

                lock (_config.ExcludedDomains)
                {
                    foreach (string domain in _config.ExcludedDomains)
                    {
                        if (uri.Authority.Contains(domain.ToLower()))
                        {
                            isExcluded = true;
                            break;
                        }
                    }
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("IsExcludedDomain", ex);
                isExcluded = true;
            }
            return isExcluded;
        }

        public bool IsExcludedFileType(string url)
        {
            bool isExcluded = false;

            lock (_config.ExcludedFileTypes)
            {
                foreach (string fileType in _config.ExcludedFileTypes)
                {
                    if (url.ToLower().EndsWith(fileType.ToLower()))
                    {
                        isExcluded = true;
                        break;
                    }
                }
            }
            return isExcluded;
        }

        public bool ShouldDownload(string url)
        {
            bool downloadMe = false;

            lock (_config.FileTypesToDownload)
            {
                foreach (string fileType in _config.FileTypesToDownload)
                {
                    if (url.ToLower().EndsWith(fileType.ToLower()))
                    {
                        downloadMe = true;
                        break;
                    }
                }
            }
            return downloadMe;
        }

        public int IdleTime()
        {
            return new Random().Next(_config.MinThreadIdleTime, _config.MaxThreadIdleTime + 1);
        }

        public bool IsWhiteListedDomain(string domain)
        {
            bool isWhiteListed = false;

            lock (_config.WhiteListedDomains)
            {
                foreach (string wlDomain in _config.WhiteListedDomains)
                {
                    if (domain.StartsWith("www."))
                        domain = domain.Remove(0, 4);

                    if (domain == wlDomain)
                    {
                        isWhiteListed = true;
                        break;
                    }
                }
            }
            return isWhiteListed;
        }

        public void Download(Uri uri)
        {
            Thread.Sleep(IdleTime());

            string filename = Path.GetFileName(uri.LocalPath);

            UriBuilder uriBuilder = new UriBuilder(uri.AbsoluteUri);
            string path = _config.DownloadFolder + Regex.Replace(Path.GetDirectoryName(uriBuilder.Path), "/", "\\");

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFileAsync(uri, path + filename);
            }
        }

        void ISpider<T>.Go()
        {
            throw new NotImplementedException();
        }

        void ISpider<T>.Go(object tag)
        {
            throw new NotImplementedException();
        }
    }
}
