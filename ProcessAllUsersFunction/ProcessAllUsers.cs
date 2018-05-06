using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using PocketSharp;
using PocketToKindle.Parsers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public static class ProcessAllUsers
    {
        private static Config _config;

        [FunctionName("ProcessAllUsers")]
        public static async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer,
            TraceWriter log,
            ExecutionContext context)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            UserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            IEmailSender emailSender = new MailgunSender(_config.MailGunSenderOptions.ApiKey, _config.MailGunSenderOptions.HostEmail);
            Sender sender = BuildSender(_config.PocketConsumerKey, _config.MercuryApiKey, emailSender, _config.ServiceDomain, _config.FunctionKey);

            UserProcessor processor = new UserProcessor(userService, sender);

            var sentUrls = await processor.ProcessAsync();

            LogSentUrls(log, sentUrls);

            return;
        }

        private static void LogSentUrls(TraceWriter log, System.Collections.Generic.IEnumerable<string> sentUrls)
        {
            if (sentUrls.Any())
            {
                StringBuilder sentUrlsStringBuilder = new StringBuilder();
                foreach (var url in sentUrls)
                {
                    sentUrlsStringBuilder.AppendLine(url);
                }

                log.Info($"Function ended at: {DateTime.Now} Sent urls: {sentUrlsStringBuilder}");
            }
            else
            {
                log.Info($"Function ended at: {DateTime.Now}: No sent urls");
            }
        }

        private static Sender BuildSender(string pocketConsumerKey, string mercuryApiKey, IEmailSender emailSender, string domain, string functionKey)
        {
            var pocketClient = new PocketClient(pocketConsumerKey);
            var mercuryParser = new MercuryParser(mercuryApiKey, domain, functionKey);
            return new Sender(pocketClient, mercuryParser, emailSender);
        }
    }
}