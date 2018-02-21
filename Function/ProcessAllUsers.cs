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
        public static async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            UserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            var emailSender = BuildEmailSender(_config.MailGunSenderOptions, _config.EmailSenderOptions);
            Sender sender = BuildSender(_config.PocketConsumerKey, _config.MercuryApiKey, emailSender);

            UserProcessor processor = new UserProcessor(userService, sender);

            await processor.ProcessAsync();

            log.Info($"Function ended at: {DateTime.Now}");

            return;
        }

        private static IEmailSender BuildSmtpEmailSender(SmtpSenderOptions emailSenderOptions)
        {
            var emailSender = new SmtpSender(emailSenderOptions);
            return emailSender;
        }

        private static IEmailSender BuildMailgunEmailSender(MailgunSenderOptions mailgunSenderOptions)
        {
            var mailgunSender = new MailgunSender(mailgunSenderOptions.ApiKey, mailgunSenderOptions.HostEmail);
            return mailgunSender;
        }

        private static Sender BuildSender(string pocketConsumerKey, string mercuryApiKey, IEmailSender emailSender)
        {
            var pocketClient = new PocketClient(pocketConsumerKey);
            var mercuryParser = new MercuryParser(mercuryApiKey);
            var sender = new Sender(pocketClient, mercuryParser, emailSender);
            return sender;
        }

        private static IEmailSender BuildEmailSender(MailgunSenderOptions mailgunSenderOptions, SmtpSenderOptions smtpSenderOptions)
        {
            IEmailSender emailSender;
            if (string.IsNullOrEmpty(mailgunSenderOptions.ApiKey))
            {
                emailSender = BuildSmtpEmailSender(_config.EmailSenderOptions);
            }
            else
            {
                emailSender = BuildMailgunEmailSender(mailgunSenderOptions);
            }

            return emailSender;
        }
    }
}