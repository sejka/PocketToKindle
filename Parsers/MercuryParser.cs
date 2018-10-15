using Core;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Parsers
{
    public class MercuryParser : IParser
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string apiUrl = "https://mercury.postlight.com/parser?url=";

        public MercuryParser(string apiKey)
        {
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<IArticle> ParseAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Url cannot be null or empty");
            }

            string requestUrl = string.Concat(apiUrl, url);
            string responseMessage;

            try
            {
                responseMessage = await httpClient.GetStringAsync(requestUrl);
            }
            catch (HttpRequestException)
            {
                return null;
            }

            IArticle article = JsonConvert.DeserializeObject<MercuryArticle>(responseMessage);

            //todo move this other place
            article = await ImageInliner.InlineImagesAsync(article);

            return article;
        }
    }
}