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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req,
            ExecutionContext context)
        {
            Config _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            var _client = new PocketClient(_config.PocketConsumerKey, callbackUri: _config.PocketRedirectUri, isMobileClient: false);
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