using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Parsers;
using PocketSharp;
using System;
using System.Threading.Tasks;

namespace Functions
{
    public static class UserSender
    {
        [FunctionName("UserSender")]
        public static async Task Run(
            [QueueTrigger("users-to-process", Connection = "")]string userJson,
            TraceWriter log,
            ExecutionContext executionContext)
        {
            var config = new ConfigBuilder(executionContext.FunctionAppDirectory).Build();

            var user = JsonConvert.DeserializeObject<User>(userJson);
            var sender = new ArticleSender(
                new PocketClient(config.PocketConsumerKey, user.AccessCode),
                new MercuryParser(config.MercuryApiKey, config.ServiceDomain),
                new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail));

            try
            {
                await sender.SendArticlesAsync(user);
            }
            catch (Exception ex)
            {
                log.Error($"Processing of user {user} failed at one of the articles", ex);
            }
            finally
            {
                var userService = UserService.BuildUserService(config.StorageConnectionString);
                await userService.UpdateLastProcessingDateAsync(user);
            }

            log.Info($"C# Queue trigger function processed: {userJson}");
        }
    }
}