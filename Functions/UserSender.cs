using Core;
using Core.EmailSenders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using PocketSharp;
using PocketToKindle.Parsers;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Functions
{
    public static class UserSender
    {
        [FunctionName("UserSender")]
        public static async Task Run(
            [QueueTrigger("users-to-process", Connection = "")]string userBinary,
            TraceWriter log,
            ExecutionContext executionContext)
        {
            var config = new ConfigBuilder(executionContext.FunctionAppDirectory).Build();

            var user = DeserializeUser(userBinary);
            var sender = new Sender(
                new PocketClient(config.PocketConsumerKey, user.AccessCode),
                new MercuryParser(config.MercuryApiKey, config.ServiceDomain, config.FunctionKey),
                new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail));

            await sender.SendAsync(user);

            var userService = UserService.BuildUserService(config.StorageConnectionString);
            await userService.UpdateLastProcessingDateAsync(user);

            log.Info($"C# Queue trigger function processed: {userBinary}");
        }

        private static User DeserializeUser(string userBinary)
        {
            var userBytes = Convert.FromBase64String(userBinary);
            return ByteArrayToUser(userBytes);
        }

        private static User ByteArrayToUser(byte[] bytes)
        {
            if (bytes == null)
                return null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                return (User)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}