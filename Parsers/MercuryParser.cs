using Newtonsoft.Json;
using PocketToKindle.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PocketToKindle.Parsers
{
    public class MercuryParser : IParser
    {
        private HttpClient httpClient = new HttpClient();
        private const string apiUrl = "https://mercury.postlight.com/parser?url=";

        public MercuryParser(string apiKey)
        {
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<Article> ParseAsync(string url)
        {
            string fullapiUrl = string.Concat(apiUrl.ToString(), url.ToString());
            var responseMessage = await httpClient.GetStringAsync(fullapiUrl);

            var article = JsonConvert.DeserializeObject<Article>(responseMessage);

            return article;
        }
    }
}