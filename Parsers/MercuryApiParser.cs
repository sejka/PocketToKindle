using Core;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Parsers
{
    public class MercuryApiParser : IParser
    {
        public readonly HttpClient _httpClient;
        private readonly ImageInliner _imageInliner;
        private readonly string _apiEndpointUrl;

        public MercuryApiParser(string apiEndpointUrl, IHttpClientFactory factory, ImageInliner imageInliner)
        {
            _apiEndpointUrl = apiEndpointUrl;
            _httpClient = factory.CreateClient();
            _imageInliner = imageInliner;
        }

        public async Task<IArticle> ParseAsync(string url)
        {
            var result = await _httpClient.GetAsync(string.Concat(_apiEndpointUrl, "/mercury?url=", url));

            if (result.IsSuccessStatusCode)
            {
                var stream = await result.Content.ReadAsStreamAsync();
                var article = await JsonSerializer.DeserializeAsync<MercuryArticle>(stream);
                return await _imageInliner.InlineImagesAsync(article, url);
            }
            else
            {
                throw new Exception($"Failed to parse article {url}, {result.Content.ReadAsStringAsync()}");
            }
        }
    }

    internal class MercuryArticle : IArticle
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("date_published")]
        public DateTime? DatePublished { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}