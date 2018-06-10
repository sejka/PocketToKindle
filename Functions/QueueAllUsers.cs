using Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Functions
{
    public static class QueueAllUsers
    {
        private static Config _config;

        [FunctionName("QueueAllUsers")]
        public static async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer,
            TraceWriter log,
            ExecutionContext context)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            UserService userService = UserService.BuildUserService(_config.StorageConnectionString);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.StorageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue usersQueue = queueClient.GetQueueReference("users-to-process");

            var userProcessor = new UserProcessor(userService, new Queue(usersQueue));
            await userProcessor.EnqueueAllUsersAsync();
        }

        private class Queue : IQueue
        {
            private readonly CloudQueue _cloudQueue;

            public Queue(CloudQueue cloudQueue)
            {
                _cloudQueue = cloudQueue;
            }

            public async Task QueueUserAsync(User user)
            {
                var payload = Convert.ToBase64String(ObjectToByteArray(user));
                await _cloudQueue.AddMessageAsync(new CloudQueueMessage(payload));
            }

            private byte[] ObjectToByteArray(object obj)
            {
                if (obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }
        }
    }
}