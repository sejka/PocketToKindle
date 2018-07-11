using Core;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Parsers
{
    public class MercuryParser : IParser
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string _domain;
        private const string apiUrl = "https://mercury.postlight.com/parser?url=";

        public MercuryParser(string apiKey, string domain)
        {
            _domain = domain;
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<IArticle> ParseAsync(string url)
        {
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
            article.AddReportLink(_domain);
            article.Content = $"<html><body><h1>{article.Title}</h1><h3>{article.DatePublished}</h3>{article.Content}</body></html>";

            return article;
        }
    }
}