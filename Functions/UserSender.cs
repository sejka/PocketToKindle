using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Parsers;
using PocketSharp;
using System.Threading.Tasks;

namespace Functions
{
    public static class UserSender
    {
        [FunctionName("UserSender")]
        public static async Task Run(
            [QueueTrigger("users-to-process", Connection = "")] string userJson,
            ILogger log,
            ExecutionContext executionContext)
        {
            var config = new ConfigBuilder(executionContext.FunctionAppDirectory).Build();

            var user = JsonConvert.DeserializeObject<User>(userJson);
            var userService = UserService.BuildUserService(config.StorageConnectionString);
            await userService.UpdateLastProcessingDateAsync(user);

            var sender = new ArticleSender(
                new PocketClient(config.PocketConsumerKey, user.AccessCode),
                new MercuryApiParser(config.MercuryParserApiEndpoint),
                new MailgunSender(config.MailgunApiKey, config.MailgunHostEmail),
                log,
                config.ServiceDomain);

            await sender.SendArticlesAsync(user);

            log.LogInformation($"C# Queue trigger function processed: {userJson}");
        }
    }
}