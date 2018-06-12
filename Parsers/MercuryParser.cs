using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace PocketToKindle.Parsers
{
    public class MercuryParser : IParser
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string _domain;
        private readonly string _functionKey;
        private const string apiUrl = "https://mercury.postlight.com/parser?url=";

        public MercuryParser(string apiKey, string domain)
        {
            _domain = domain;
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<MercuryArticle> ParseAsync(string url)
        {
            string requestUrl = string.Concat(apiUrl, url);
            var responseMessage = await httpClient.GetStringAsync(requestUrl);

            var article = JsonConvert.DeserializeObject<MercuryArticle>(responseMessage);

            //todo move this other place
            article = await ImageInliner.InlineImagesAsync(article);
            article.AddReportLink(_domain, _functionKey);

            return article;
        }
    }
}