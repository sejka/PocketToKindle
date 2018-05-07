using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PocketSharp;

namespace Functions.Web
{
    public static class GetRegistrationInfo
    {
        [FunctionName("GetRegistrationInfo")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
            TraceWriter log,
            ExecutionContext context)
        {
            Config _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            log.Info("C# HTTP trigger function processed a request.");

            var _client = new PocketClient(_config.PocketConsumerKey, callbackUri: _config.PocketRedirectUri);
            string requestCode = await _client.GetRequestCode();

            var result = new
            {
                RequestCode = requestCode,
                RegistrationLink = _client.GenerateRegistrationUri(requestCode)
            };

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }
}