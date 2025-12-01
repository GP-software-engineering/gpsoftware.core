using System;
using System.Collections.Generic;
using System.Net;
using GPSoftware.Core.Extensions;

namespace GPSoftware.Core.Net {

    public static class HttpRequestExtension {

        public const string CONSENT_COOKIE_NAME = "cookieConsent";

        /// <summary>
        ///     Get root url (Host Name + Port No)
        ///     (see "https://stackoverflow.com/questions/2142910/whats-the-difference-between-uri-host-and-uri-authority")
        /// </summary>
        public static string GetRootUrl(this Uri uri) {
            return uri.GetLeftPart(UriPartial.Authority);
        }

        /// <summary>
        ///     Get root url (Host Name + Port No)
        ///     <see cref="https://stackoverflow.com/questions/2142910/whats-the-difference-between-uri-host-and-uri-authority"/>
        /// </summary>
        public static string GetRootUrl(this WebRequest request) {
            return request.RequestUri.GetRootUrl();
        }

        /// <summary>
        ///     Get the canonical Url (see "https://sitechecker.pro/it/canonical-url/")
        /// </summary>
        public static string GetCanonicalUrl(this Uri uri) {
            var canonical = string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.AbsolutePath).ToLower();
            if (canonical.Count('/') > 3) canonical = canonical.TrimEnd('/');
            if (canonical.EndsWith("/index")) canonical = canonical.Substring(0, canonical.Length - 6);
            return canonical;
        }

        /// <summary>
        ///     Get the canonical Url (<see cref="https://sitechecker.pro/it/canonical-url/"/>)
        /// </summary>
        public static string GetCanonicalUrl(this WebRequest request) {
            return request.RequestUri.GetCanonicalUrl();
        }

        /// <summary>
        ///     Check current browser's request has the do not track request
        /// </summary>
        public static bool HasDoNotTrackHeader(this WebRequest request) {
            // check the Do Not Track header value, this can have the value "0" or "1"
            string? dnt = request.Headers.Get("DNT");
            return !string.IsNullOrEmpty(dnt) && (dnt == "1");
        }

        /// <summary>
        ///     Check the current request is from the most important crawlers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsSearchCrawler(this WebRequest request) {
            // HttpWebRequest is deprecated in .NET 6+, but we handle it safely here
            string? userAgent = (request is HttpWebRequest httpRequest) 
                ? httpRequest.UserAgent 
                : request.Headers.Get("User-Agent");

            return (!string.IsNullOrEmpty(userAgent) && s_crawlerZ.Contains(userAgent));
            //return (!string.IsNullOrEmpty(userAgent) && (Array.IndexOf(s_crawlers, userAgent) >= 0));
        }

        private static readonly SortedSet<string> s_crawlerZ = new SortedSet<string>(s_crawlers, StringComparer.OrdinalIgnoreCase);
        
        private static readonly string[] s_crawlers = new[] {
            "AcoonBot",
            "AhrefsBot",
            "Baiduspider",
            "Bingbot",
            "DuckDuckBot",
            "ExaBot",
            "Exabot",
            "Googlebot",
            "Googlebot-Image",
            "Googlebot-News",
            "SeznamBot",
            "Slurp",
            "Speedy Spider",
            "Vagabondo",
            "Yahoo! Slurp",
            "YandexBot",
            "YandexImages",
            "bingbot",
            "ccbot",
            "facebookexternalhit",
            "facebot",
            "ia_archiver",
            "msnbot",
        };

    }
}
