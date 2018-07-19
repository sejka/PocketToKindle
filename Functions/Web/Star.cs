using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using PocketSharp;
using System.Threading.Tasks;

namespace Functions.Web
{
    public static class Star
    {
        [FunctionName("Star")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req,
            TraceWriter log,
            ExecutionContext context)
        {
            Config _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            string articleId = req.Query["articleId"];
            string userHash = req.Query["userHash"];

            var userService = UserService.BuildUserService(_config.StorageConnectionString);
            var user = await userService.FindUserWithHash(userHash);

            if (user == null)
            {
                return new BadRequestObjectResult("invalid user hash");
            }

            var pocketClient = new PocketClient(_config.PocketConsumerKey, user.AccessCode);
            var article = await pocketClient.Get(articleId);

            if (article == null)
            {
                return new BadRequestObjectResult("invalid article id");
            }

            var success = await pocketClient.Favorite(article);

            //todo have html results
            return success
                ? (ActionResult)new OkObjectResult("Archiving successful")
                : new BadRequestObjectResult("Something went wrong");
        }
    }
}