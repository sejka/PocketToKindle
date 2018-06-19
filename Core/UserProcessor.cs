using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class UserProcessor
    {
        private readonly IUserService _userService;
        private readonly IQueue _usersQueue;

        public UserProcessor(IUserService userService, IQueue usersQueue)
        {
            _userService = userService;
            _usersQueue = usersQueue;
        }

        public async Task EnqueueAllUsersAsync()
        {
            var processTasks = new List<Task>();

            do
            {
                var userBatch = await _userService.GetUserBatch();

                if (userBatch.Any())
                {
                    processTasks.Add(EnqueueBatchAsync(userBatch, _usersQueue));
                }
            } while (_userService.GetContinuationToken() != null);

            await Task.WhenAll(processTasks);
        }

        private static async Task EnqueueBatchAsync(IEnumerable<User> users, IQueue queue)
        {
            var processorTasks = users
                .AsParallel()
                .Select(async user => await queue.QueueUserAsync(user));

            await Task.WhenAll(processorTasks);
        }
    }
}