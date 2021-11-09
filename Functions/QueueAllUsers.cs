using Core;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace Functions
{
    public static class QueueAllUsers
    {
        private static Config _config;

        [FunctionName("QueueAllUsers")]
        public static async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer,
            ExecutionContext context,
            [Queue("users-to-process")] ICollector<User> myDestinationQueue)
        {
            _config = new ConfigBuilder(context.FunctionAppDirectory).Build();

            var userService = UserService.BuildUserService(_config.StorageConnectionString);

            var userProcessor = new UserProcessor(userService, myDestinationQueue);
            await userProcessor.EnqueueAllUsersAsync();
        }
    }
}