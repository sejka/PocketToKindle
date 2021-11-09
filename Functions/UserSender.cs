using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Parsers;
using PocketSharp;
using System.Text.Json;
using System.Threading.Tasks;

namespace Functions
{
    public static class UserSender
    {
        [FunctionName("UserSender")]
        public static async Task Run(
            [QueueTrigger("users-to-process")] string userJson,
            ILogger log)
        {
            var config = new ConfigBuilder("").Build();

            var user = JsonSerializer.Deserialize<User>(userJson);
            var userService = UserService.BuildUserService(config.StorageConnectionString);
            await userService.UpdateLastProcessingDateAsync(user);

            var sender = new ArticleSender(
                new PocketClient(config.PocketConsumerKey, user.AccessCode),
                new MercuryApiParser(config.MercuryParserApiEndpoint),
                new SendgridSender(config.SendgridApiKey, config.MailgunHostEmail),
                userService,
                log,
                config.ServiceDomain);

            await sender.SendArticlesAsync(user);

            log.LogInformation($"C# Queue trigger function processed: {userJson}");
        }
    }
}