using Core;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

[assembly: InternalsVisibleTo("Tests")]

namespace Parsers
{
    public class ImageInliner
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageInliner> _logger;

        public ImageInliner(IHttpClientFactory factory, ILogger<ImageInliner> logger)
        {
            _httpClient = factory.CreateClient();
            _logger = logger;
        }

        public async Task<IArticle> InlineImagesAsync(IArticle article, string url)
        {
            var articleContent = new HtmlDocument();
            articleContent.LoadHtml(HttpUtility.HtmlDecode(article.Content));

            var images = articleContent.DocumentNode.SelectNodes("//img");
            var iframeParents = articleContent.DocumentNode.SelectNodes("//iframe");
            if (iframeParents != null)
            {
                foreach (var iframeParent in iframeParents)
                {
                    iframeParent.Remove();
                }
            }

            if (images == null)
            {
                return article;
            }

            var articleImageConversionTasks = new List<Task>();

            foreach (var image in images)
            {
                if (image.Attributes.Contains("src") || image.Attributes.Contains("srcset"))
                {
                    articleImageConversionTasks.Add(InlineImageAsync(image, GetDomainNameOfUrlString(url)));
                }
                else
                {
                    _logger.LogInformation($"image without src or srcset attributes: {string.Join(",", image.Attributes.Select(x => $"{x.Name}={x.Value}"))}");
                    image.Remove();
                }
            }

            await Task.WhenAll(articleImageConversionTasks);

            article.Content = articleContent.DocumentNode.InnerHtml;
            return article;
        }

        private async Task InlineImageAsync(HtmlNode image, string articleDomain)
        {
            string imageUrl;
            if (image.Attributes.Contains("srcset") && !image.Attributes.Contains("src"))
            {
                imageUrl = SelectImageFromSrcset(image.Attributes["srcset"].Value);
            }
            else
            {
                imageUrl = image.Attributes["src"].Value;
            }

            if (image.Attributes.Contains("srcset"))
            {
                image.Attributes.Remove("srcset");
            }

            if (imageUrl.StartsWith("data:image")) // already inlined
            {
                return;
            }
            else if (imageUrl.StartsWith("/")) // relative path
            {
                imageUrl = articleDomain + imageUrl;
            }
            else if (!imageUrl.Contains('/')) // just filename
            {
                imageUrl = articleDomain + '/' + imageUrl;
            }
            imageUrl = HttpUtility.UrlDecode(imageUrl);
            string imageAsBase64 = await GetImageAsBase64Async(imageUrl);

            if (imageAsBase64 == string.Empty)
            {
                image.Remove();
                _logger.LogError($"couldn't parse image {articleDomain}, {imageUrl}");
            }

            var extension = GetFileExtensionFromUrl(imageUrl);
            if (extension == "jpg" || extension == "jpeg")
            {
                extension = "jpeg";
            }
            else if (extension != "png" && extension != "gif")
            {
                _logger.LogInformation($"unknown extension {extension}");
                image.Remove();
            }

            image.Attributes["src"].Value = $"data:image/{extension};base64,{imageAsBase64}";
        }

        public static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.') + 1).ToLower() : "";
        }

        private static string SelectImageFromSrcset(string srcsetValue)
        {
            int firstSpaceIndex = srcsetValue.IndexOf(' ');
            return srcsetValue.Substring(0, firstSpaceIndex);
        }

        private static string GetDomainNameOfUrlString(string urlString)
        {
            return new Uri(urlString).GetLeftPart(UriPartial.Authority);
        }

        public async Task<string> GetImageAsBase64Async(string url)
        {
            _logger.LogInformation($"downloading {url}");
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                _logger.LogError($"error downloading {url}: {e.Message}");
            }
            return String.Empty;
        }
    }
}