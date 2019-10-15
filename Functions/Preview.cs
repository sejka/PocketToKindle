using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Functions.Web;
using System.Net.Http;
using System.Net;
using Parsers;
using Core;

namespace Functions
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