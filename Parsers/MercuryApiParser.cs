using Core;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Parsers
{
    public class MercuryApiParser : IParser
    {
        public static HttpClient _httpClient = new HttpClient();
        private readonly string _apiEndpointUrl;

        public MercuryApiParser(string apiEndpointUrl)
        {
            _apiEndpointUrl = apiEndpointUrl;
        }

        public async Task<IArticle> ParseAsync(string url)
        {
            var result = await _httpClient.GetAsync(string.Concat(_apiEndpointUrl, url));

            if (result.IsSuccessStatusCode)
            {
                string json = await result.Content.ReadAsStringAsync();
                var article = JsonConvert.DeserializeObject<MercuryArticle>(json);
                return await ImageInliner.InlineImagesAsync(article);
            }
            else
            {
                throw new Exception($"Failed to parse article {url}, {result.Content.ReadAsStringAsync()}");
            }
        }
    }

    internal class MercuryArticle : IArticle
    {
        public string Content { get; set; }

        [JsonProperty("date_published")]
        public DateTime? DatePublished { get; set; }

        public string Title { get; set; }
        public string Url { get; set; }
    }
}