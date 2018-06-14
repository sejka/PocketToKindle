using Core;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Tests")]

namespace Parsers
{
    internal static class ImageInliner
    {
        private static HttpClient _httpClient = new HttpClient();

        public static async Task<IArticle> InlineImagesAsync(IArticle article)
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
            string imageUrl;
            if (image.Attributes.Contains("srcset"))
            {
                imageUrl = SelectImageFromSrcset(image.Attributes["srcset"].Value);
                image.Attributes.Remove("srcset");
            }
            else
            {
                imageUrl = image.Attributes["src"].Value;
            }

            string imageAsBase64 = await GetImageAsBase64Async(imageUrl);

            if (imageAsBase64 == string.Empty)
            {
                image.Remove();
            }

            image.Attributes["src"].Value = $"data:image/jpeg;base64,{imageAsBase64}";
        }

        private static string SelectImageFromSrcset(string srcsetValue)
        {
            int firstSpaceIndex = srcsetValue.IndexOf(' ');
            return srcsetValue.Substring(0, firstSpaceIndex);
        }

        private async static Task<string> GetImageAsBase64Async(string url)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}