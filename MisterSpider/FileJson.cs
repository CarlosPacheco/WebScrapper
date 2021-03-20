using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MisterSpider
{
    public class FileJson
    {
        /// <summary>
        /// Lock for write in the file.
        /// </summary>
        private readonly object logLock = new object();
        private string file { get; set; }
        private string path { get; set; }
        private List<string> LogFileLines { get; set; }
        public FileJson(string fileName, string folderpath)
        {
            file = fileName;
            path = folderpath;
        }

        public void Write(object entry)
        {
            //  Task.Factory.StartNew(() =>
            //  {
            lock (logLock)
            {
                string line = JsonSerializer.Serialize(entry);
                string filepath = string.Format("{0}{1}", path, file);
                if (File.Exists(filepath))
                {
                    LogFileLines = LogFileLines ?? File.ReadLines(filepath).ToList();
                    if (LogFileLines.Any())
                    {
                        //change de ] to ,
                        var lastLine = LogFileLines[LogFileLines.Count -1];
                        LogFileLines[LogFileLines.Count -1] = string.Format("{0},", lastLine.Substring(0, lastLine.Length - 1));
                        // add the new line
                        LogFileLines.Add(string.Format("{0}]", line));
                        File.WriteAllLines(filepath, LogFileLines);
                        return;
                    }
                }
                File.AppendAllText(filepath, string.Format("[{0}]{1}", line, Environment.NewLine));
            }
            //  });
        }
    }
}
