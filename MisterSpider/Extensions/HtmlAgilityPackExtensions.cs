using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace MisterSpider.Extensions
{
    public static class HtmlAgilityPackExtensions
    {
        public static string Extract(this HtmlNode value, bool useHtmlDecode = true)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (useHtmlDecode)
            {
                return WebUtility.HtmlDecode(value.InnerHtml);
            }

            return value.InnerHtml;
        }

        public static string Extract(this HtmlNode value, string attributeName, bool useHtmlDecode = true)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var attri = value.Attributes[attributeName];
            if (attri == null)
            {
                return string.Empty;
            }

            if (useHtmlDecode)
            {
                return WebUtility.HtmlDecode(attri.Value);
            }

            return attri.Value;
        }

        public static List<string> Extract(this HtmlNode value, params string[] attributeNames)
        {
            List<string> list = new List<string>();
            if (value == null)
            {
                return list;
            }

            foreach (string attriName in attributeNames)
            {
                HtmlAttribute attri = value.Attributes[attriName];
                if (attri != null)
                {
                    list.Add(attri.Value);
                }
            }

            return list;
        }

        public static string Extract(this HtmlNodeCollection value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string val = string.Empty;

            foreach (HtmlNode item in value)
            {
                val += item.Extract();
            }
            return val;
        }

        public static List<string> Extract(this HtmlNodeCollection value, string attributeName)
        {
            List<string> list = new List<string>();
            if (value == null)
            {
                return list;
            }

            foreach (HtmlNode item in value)
            {
                HtmlAttribute attri = item.Attributes[attributeName];
                if (attri != null)
                {
                    list.Add(attri.Value);
                }
            }
            return list;
        }

        public static List<string> Extract(this HtmlNodeCollection value, params string[] attributeNames)
        {
            List<string> list = new List<string>();
            if (value == null)
            {
                return list;
            }

            foreach (HtmlNode item in value)
            {
                foreach (string attriName in attributeNames)
                {
                    HtmlAttribute attri = item.Attributes[attriName];
                    if (attri != null)
                    {
                        list.Add(attri.Value);
                    }
                }
            }
            return list;
        }

        public static string ExtractDecode(this HtmlNode value, bool useHtmlDecode = true)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return Regex.Replace(value.Extract(useHtmlDecode), @"\\u([\dA-Fa-f]{4})", v => ((char)Convert.ToInt32(v.Groups[1].Value, 16)).ToString());
        }

        public static string ExtractScrubHtml(this HtmlNodeCollection value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string val = string.Empty;

            foreach (HtmlNode item in value)
            {
                val += RemoveHtmlTags(item.Extract());
            }
            return val;
        }

        public static string ExtractScrubHtml(this HtmlNode value, bool useHtmlDecode = true)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return RemoveHtmlTags(value.Extract(useHtmlDecode));
        }

        public static string RemoveHtmlTags(string value)
        {
            return Regex.Replace(Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim(), @"\s{2,}", " ");
        }
    }
}
