using Core;
using MailKit.Net.Smtp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using PocketSharp;
using PocketToKindle.Parsers;
using System;

namespace Function
{
    public static class ProcessAllUsers
    {
        private static Config _config;

        [FunctionName("ProcessAllUsers")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            UserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            Sender sender = BuildSender(_config.PocketConsumerKey, _config.MercuryApiKey, _config.EmailSenderOptions);

            UserProcessor processor = new UserProcessor(userService, sender);

            processor.ProcessAsync().Wait();

            log.Info($"Function ended at: {DateTime.Now}");

            return;
        }

        private static EmailSender BuildEmailSender(EmailSenderOptions emailSenderOptions)
        {
            var emailSender = new EmailSender(emailSenderOptions);
            return emailSender;
        }

        private static Sender BuildSender(string pocketConsumerKey, string mercuryApiKey, EmailSenderOptions emailSenderOptions)
        {
            var pocketClient = new PocketClient(pocketConsumerKey);
            var mercuryParser = new MercuryParser(mercuryApiKey);
            EmailSender emailSender = BuildEmailSender(_config.EmailSenderOptions);
            var sender = new Sender(pocketClient, mercuryParser, emailSender);
            return sender;
        }
    }
}