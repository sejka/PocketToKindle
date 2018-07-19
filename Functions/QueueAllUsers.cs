using Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
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

            var userService = UserService.BuildUserService(_config.StorageConnectionString);

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
                await _cloudQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(user)));
            }
        }
    }
}