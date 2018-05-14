using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PocketSharp;
using PocketToKindle.Parsers;
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
            var sender = new Sender(
                new PocketClient(config.PocketConsumerKey, user.AccessCode),
                new MercuryParser(config.MercuryApiKey, config.ServiceDomain, config.FunctionKey),
                new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail));

            await sender.SendAsync(user);

            log.Info($"C# Queue trigger function processed: {userJson}");
        }
    }
}