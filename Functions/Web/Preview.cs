using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Parsers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.Web
{
    public static class Preview
    {
        [FunctionName("Preview")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            if (!req.Query.ContainsKey("url"))
            {
                return new HtmlResponseMessage(HttpStatusCode.BadRequest, "you need to provide url");
            }

            var config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            var parser = new MercuryApiParser(config.MercuryParserApiEndpoint);

            var article = await parser.ParseAsync(req.Query["url"]);

            return new HtmlResponseMessage(HttpStatusCode.OK, article.Content);
        }
    }
}