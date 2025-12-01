using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using GPSoftware.Core.Extensions;

namespace GPSoftware.Core.Helpers {

    public static class HtmlParserHelper {

        //public static readonly Regex s_imgTagRegex = new Regex("<img[^>]+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex s_imgTagRegex = new Regex("<img (.*?)(/>|>)]*?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex s_hrefTagRegex = new Regex("<a (.*?)(/>|>)]*?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex s_base64Regex = new Regex("data:[^/]+/(?<ext>[a-z]+);base64,(?<base64>.+)", RegexOptions.IgnoreCase);

        private static readonly string[] s_allowedExtensions = new[] {
            ".jpg", ".jpeg", ".gif", ".png", ".webp" }; // Make sure it is an image that can be processed

        /// <summary>
        ///     Analyze an html content and return the 'a' tags (hrefs).
        /// </summary>
        /// <returns>
        ///     A collection of string matching found by the search. If no matches are found, the method
        ///     returns an empty collection of strings.
        /// </returns>
        public static ICollection<string> ExtractHrefTags(string htmlContent) => ExtractTagsFromHTML(htmlContent, s_hrefTagRegex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public static ICollection<string> ExtractHrefUrls(string htmlContent) {
            var output = new HashSet<string>();
            // Guard clause for empty content
            if (string.IsNullOrWhiteSpace(htmlContent)) return output;

            XmlDocument doc = new XmlDocument();
            foreach (var tag in ExtractHrefTags(htmlContent)) {
                // Ensure valid XML wrapping
                string xmlSnippet = "<root>" + (tag.EndsWith("/>") ? tag : tag.TrimEnd('>') + "/>") + "</root>";
                
                try {
                    doc.LoadXml(xmlSnippet);
                    
                    // Safe navigation using DocumentElement (root) and null-conditional operators
                    var imgNode = doc.DocumentElement?.FirstChild;
                    
                    string? fileUrl = GetSourceUrlFromXmlNode(imgNode);
                    if (!string.IsNullOrEmpty(fileUrl)) {
                         output.AddIfNotContains(fileUrl!); // fileUrl is not null here checked by string.IsNullOrEmpty
                    }
                } 
                catch (XmlException) {
                    // Ignore malformed XML tags extracted by regex
                    continue;
                }
            }

            return output;
        }

        /// <summary>
        ///     Analyze an html content and return 'img' tags.
        /// </summary>
        /// <returns>A never null collection of strings matching found by the search.</returns>
        public static ICollection<string> ExtractImgTags(string htmlContent) => ExtractTagsFromHTML(htmlContent, s_imgTagRegex);

        /// <summary>
        ///     Return the non-empty 'data-filename' or 'src' from all the 'img' tags of the passed HTML
        ///     Eg.
        ///         <![CDATA[
        ///            <img src="/a/b/filename.jpg" data-filename="/mypath/filename.jpg"> returns "/mypath/filename.jpg"
        ///            <img src="/a/b/filename.jpg"> returns "/a/b/filename.jpg"
        ///            <img src=""> is skipped
        ///         ]]>
        /// </summary>
        /// <returns>A never null collection of strings matching found by the search.</returns>
        public static ICollection<string> ExtractImgUrl(string htmlContent) {
            var output = new HashSet<string>();
            // Guard clause for empty content
            if (string.IsNullOrWhiteSpace(htmlContent)) return output;

            XmlDocument doc = new XmlDocument();
            foreach (var tag in ExtractImgTags(htmlContent)) {
                string xmlSnippet = "<root>" + (tag.EndsWith("/>") ? tag : tag.TrimEnd('>') + "/>") + "</root>";
                
                try {
                    doc.LoadXml(xmlSnippet);
                    
                    // Safe navigation to avoid NullReferenceException
                    var imgNode = doc.DocumentElement?.FirstChild;

                    string? fileUrl = GetSourceUrlFromXmlNode(imgNode);
                    if (!string.IsNullOrEmpty(fileUrl)) {
                        output.AddIfNotContains(fileUrl!);
                    }
                }
                catch (XmlException) {
                    // Ignore malformed XML tags
                    continue;
                }
            }

            return output;
        }

        /// <summary>
        ///     Return the non-empty 'data-filename' or 'src' in the loca OS format of all the 'img' tags of the passed HTML.
        ///     Skip src url such as src="http://...". 
        ///     Eg.
        ///         <![CDATA[
        ///            <img src="/a/b/filename.jpg" data-filename="/mypath/filename.jpg"> returns on Windows: "\\mypath\\filename.jpg"
        ///            <src img="/a/b/filename.jpg"> returns on Unix: "/a/b/filename.jpg"
        ///            <img src=""> is skipped
        ///         ]]>
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <param name="trimFilePath">if true, return just the file name. E.g. "filename.jpg"</param>
        /// <remarks>Skip non-local src url such as src="http://...".</remarks>
        /// <returns>A never null collection of strings matching found by the search.</returns>
        public static ICollection<string> ExtractLocalImgPaths(string htmlContent, bool trimFilePath = false) {
            var output = new HashSet<string>();
            // Guard clause for empty content
            if (string.IsNullOrWhiteSpace(htmlContent)) return output;

            XmlDocument doc = new XmlDocument();
            foreach (var tag in ExtractImgTags(htmlContent)) {
                string xmlSnippet = "<root>" + (tag.EndsWith("/>") ? tag : tag.TrimEnd('>') + "/>") + "</root>";
                
                try {
                    doc.LoadXml(xmlSnippet);

                    // Safe navigation using DocumentElement
                    var imgNode = doc.DocumentElement?.FirstChild;

                    string? fileUrl = GetSourceUrlFromXmlNode(imgNode);

                    if (!string.IsNullOrEmpty(fileUrl)) {
                        // fileUrl is not null here
                        
                        // fix possilbe wrong path separators
                        fileUrl = fileUrl!.Replace('\\', '/');

                        // skip external resources and non-image files
                        if (fileUrl.Contains("://") || fileUrl.StartsWith("//")) continue;
                        
                        // Safety check: GetExtension might throw on invalid paths or return null/empty
                        var extension = Path.GetExtension(fileUrl);
                        if (string.IsNullOrEmpty(extension) || !s_allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) continue;

                        // The HTML editor creates base64 DataURIs which we'll have to convert to image files on disk

                        //var base64Match = base64Regex.Match(srcNode.Value);
                        //if (base64Match.Success) {
                        //    byte[] bytes = Convert.FromBase64String(base64Match.Groups["base64"].Value);
                        //    srcNode.Value = await SaveFile(bytes, fileNameNode.Value).ConfigureAwait(false);

                        //    img.Attributes.Remove(fileNameNode);
                        //    htmlContent = htmlContent.Replace(match.Value, img.OuterXml);
                        //}

                        output.AddIfNotContains(trimFilePath ? Path.GetFileName(fileUrl) : fileUrl);
                    }
                }
                catch (Exception) {
                     // Catch XmlException or Path exceptions to avoid crashing on bad input
                     continue;
                }
            }

            return output;
        }

        /// <summary>
        ///      A collection of string matching found by the search. If no matches are found, the method
        ///      returns an empty collection string.
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <param name="tagRegex"></param>
        private static ICollection<string> ExtractTagsFromHTML(string htmlContent, Regex tagRegex) {
            if (tagRegex is null) throw new ArgumentNullException(nameof(tagRegex));
            if (string.IsNullOrEmpty(htmlContent)) return new List<string>(); // Return empty if content is null

            return tagRegex.Matches(htmlContent)
                .Cast<Match>()
                .Select(match => match.Value)
                .ToArray();
        }

        /// <summary>
        /// Helper method to safely extract the URL from an XML node.
        /// Prioritizes 'data-filename', falls back to 'src'.
        /// Returns null if node is invalid or attributes are missing.
        /// </summary>
        private static string? GetSourceUrlFromXmlNode(XmlNode? node) {
            if (node?.Attributes == null) return null;

            // Safe access using null-conditional operator '?' on Attributes collection
            // and null-coalescing '??' to check src if data-filename is missing.
            var attribute = node.Attributes["data-filename"] ?? node.Attributes["src"];
            
            // Return trimmed value or null if attribute doesn't exist/has no value
            return attribute?.Value?.Trim()?.Replace('\\', '/'); 
        }

    }
}
