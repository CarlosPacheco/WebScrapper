using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace MisterSpider
{
    public class ConfigOptions
    {
        public const string Position = "Spider";

        /// <summary>
        /// MaximumDepth refers to the link depth from the root page that should be crawled. 
        /// For example, if MaximumDepth is set to 0 then only the root URL will be crawled.
        /// If MaximumDepth is set to 1 then the root URL will be crawled along with every URL
        /// found on the root page. Setting the depth higher than 3 or 4 without using a
        /// whitelist may cause the crawler to queue tens of thousands of web pages and run for 
        /// a very long time
        /// </summary>
        public int MaximumDepth { get; set; }

        /// <summary>
        /// MaximumThreads refers to the maximum number of crawling processes that should run
        /// concurrently.It is recommended that this number be kept very low(i.e.below 10)
        /// to avoid putting strain on the web servers of the sites being crawled
        /// </summary>
        public int MaximumThreads { get; set; }

        /// <summary>
        /// Set UseLogging to True to generate a log file for the crawling session. Otherwise
        /// set to False.Console output is not affected by this setting
        /// </summary>
        public bool UseLogging { get; set; }

        /// <summary>
        /// Set UseWhiteList to True to restrict the crawler to crawling *only* the domains listed
        /// under WhiteListedDomains.Any URL with a domain not on the whitelist will be skipped
        /// </summary>
        public bool UseWhiteList { get; set; }

        public bool ShouldSleep { get; set; }

        /// <summary>
        /// MinThreadIdleTime and MaxThreadIdleTime are used to throttle the crawler. A thread will
        /// sleep for a random period of time between the minimum and maximum idle times before fetching
        /// a new page and before downloading a file.Time is in milliseconds.
        /// </summary>
        public int MinThreadIdleTime { get; set; }

        /// <summary>
        /// MinThreadIdleTime and MaxThreadIdleTime are used to throttle the crawler. A thread will
        /// sleep for a random period of time between the minimum and maximum idle times before fetching
        /// a new page and before downloading a file.Time is in milliseconds.
        /// </summary>
        public int MaxThreadIdleTime { get; set; }

        /// <summary>
        /// The location on the hard drive where downloaded files will be saved
        /// </summary>
        public string DownloadFolder { get; set; }

        /// <summary>
        /// The location on the hard drive where the log file will be saved
        /// </summary>
        public string LogFolder { get; set; }

        /// <summary>
        /// List the file types the crawler should download here. File types listed should be in this
        /// format using "|" as a delimiter: 
        /// .jpg|.mp3|.pdf
        /// </summary>
        [JsonConverter(typeof(FileTypeJsonConverter))]
        public IList<string> FileTypesToDownload { get; set; }

        /// <summary>
        /// List the file types the crawler should ignore here. File types listed should be in this
        /// format using "|" as a delimiter: 
        /// .jpg|.mp3|.pdf
        /// </summary>
        [JsonConverter(typeof(FileTypeJsonConverter))]
        public IList<string> ExcludedFileTypes { get; set; }

        /// <summary>
        /// List the domains crawler should ignore here. Any URL with a domain listed here will be skipped.
        /// URLs should be in this format: 
        /// google.com
        /// facebook.com
        /// Each domain should be indented and seperated by a new line.Do not include "http://www"
        /// </summary>
        public IList<string> ExcludedDomains { get; set; }

        /// <summary>
        /// List the domains the crawler should *only* visit here. Any URL with a domain not listed here will 
        /// be skipped if the UseWhiteList setting is set to True.
        /// URLs should be in this format: 
        /// google.com
        /// facebook.com
        /// Each domain should be properly indented and seperated by a new line.Do not include "http://www"
        /// </summary>
        public IList<string> WhiteListedDomains { get; set; }

        public IList<string> ClassTypes { get; set; }

        public bool SaveErrorItens { get; set; }

        public bool TryAgainOnError { get; set; }

        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint IpAddress { get; set; }
    }
}
