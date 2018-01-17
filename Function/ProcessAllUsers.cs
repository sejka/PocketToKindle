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
        private static Config _config = new ConfigBuilder().Build();

        [FunctionName("ProcessAllUsers")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            UserService userService = BuildUserService();
            Sender sender = BuildSender(_config.PocketConsumerKey, _config.MercuryApiKey, _config.EmailSenderOptions);

            UserProcessor processor = new UserProcessor(userService, sender);

            processor.ProcessAsync().Wait();

            log.Info($"Function ended at: {DateTime.Now}");

            return;
        }

        private static EmailSender BuildEmailSender(EmailSenderOptions emailSenderOptions)
        {
            var smtpClient = new SmtpClient();
            smtpClient.Connect(emailSenderOptions.Host, Convert.ToInt32(emailSenderOptions.Port), true);
            smtpClient.Authenticate(emailSenderOptions.Login, emailSenderOptions.Password);
            var emailSender = new EmailSender(smtpClient);
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

        private static UserService BuildUserService()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.StorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable userCloudTable = tableClient.GetTableReference("users");
            var userService = new UserService(userCloudTable);
            return userService;
        }
    }
}