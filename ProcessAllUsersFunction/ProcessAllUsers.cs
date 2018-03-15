using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using PocketSharp;
using PocketToKindle.Parsers;
using System;
using System.Threading.Tasks;

namespace Function
{
    public static class ProcessAllUsers
    {
        private static Config _config;

        [FunctionName("ProcessAllUsers")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            TraceWriter log,
            ExecutionContext context)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            UserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            IEmailSender emailSender = new SmtpSender(_config.EmailSenderOptions);
            Sender sender = BuildSender(_config.PocketConsumerKey, _config.MercuryApiKey, emailSender);

            UserProcessor processor = new UserProcessor(userService, sender);

            var sentUrls = await processor.ProcessAsync();

            log.Info($"Function ended at: {DateTime.Now}");

            return;
        }

        private static Sender BuildSender(string pocketConsumerKey, string mercuryApiKey, IEmailSender emailSender)
        {
            var pocketClient = new PocketClient(pocketConsumerKey);
            var mercuryParser = new MercuryParser(mercuryApiKey);
            var sender = new Sender(pocketClient, mercuryParser, emailSender);
            return sender;
        }
    }
}