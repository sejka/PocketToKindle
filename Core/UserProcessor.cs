using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class UserProcessor
    {
        private readonly IUserService _userService;
        private readonly ICollector<User> _usersQueue;

        public UserProcessor(IUserService userService, ICollector<User> usersQueue)
        {
            _userService = userService;
            _usersQueue = usersQueue;
        }

        public async Task EnqueueAllUsersAsync()
        {
            do
            {
                var userBatch = await _userService.GetUserBatch();

                if (userBatch.Any())
                {
                    EnqueueBatchAsync(userBatch, _usersQueue);
                }
                userBatch = new LinkedList<User>();
            } while (_userService.GetContinuationToken() != null);
        }

        private static void EnqueueBatchAsync(IEnumerable<User> users, ICollector<User> queue)
        {
            foreach (var user in users)
            {
                queue.Add(user);
            }
        }
    }
}