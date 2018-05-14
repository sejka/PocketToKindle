using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PocketSharp;
using PocketSharp.Models;
using System;
using System.IO;

namespace Functions.Web
{
    public static class Register
    {
        [FunctionName("Register")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext context)
        {
            Config _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            log.Info("C# HTTP trigger function processed a request.");
            var _client = new PocketClient(_config.PocketConsumerKey, callbackUri: _config.PocketRedirectUri);

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            RegisterRequest request = JsonConvert.DeserializeObject<RegisterRequest>(requestBody);

            PocketUser pocketUser = await _client.GetUser(request.RequestCode);

            IUserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            userService.AddUser(new User
            {
                AccessCode = pocketUser.Code,
                PocketUsername = pocketUser.Username,
                KindleEmail = request.KindleEmail,
                LastProcessingDate = DateTime.UtcNow
            });

            return new OkObjectResult("yasss");

            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private class RegisterRequest
        {
            public string RequestCode { get; set; }
            public string KindleEmail { get; set; }
        }
    }
}