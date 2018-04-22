using HtmlAgilityPack;
using PocketToKindle.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Tests")]

namespace Core
{
    internal static class ImageInliner
    {
        private static HttpClient _httpClient = new HttpClient();

        public static async Task<Article> InlineImagesAsync(Article article)
        {
            var articleContent = new HtmlDocument();
            articleContent.LoadHtml(article.Content);

            var articleImages = articleContent.DocumentNode.SelectNodes("//img");

            if (articleImages == null)
            {
                return article;
            }

            var articleImageConversionTasks = new List<Task>();

            foreach (var articleImage in articleImages)
            {
                articleImageConversionTasks.Add(InlineImageAsync(articleImage));
            }

            await Task.WhenAll(articleImageConversionTasks);

            article.Content = articleContent.DocumentNode.InnerHtml;
            return article;
        }

        private static async Task InlineImageAsync(HtmlNode image)
        {
            string imageInBase64 = await GetImageAsBase64UrlAsync(image.Attributes["src"].Value);

            image.Attributes["src"].Value = $"data:image/jpeg;base64,{imageInBase64}";
        }

        private async static Task<string> GetImageAsBase64UrlAsync(string url)
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            return Convert.ToBase64String(bytes);
        }
    }
}