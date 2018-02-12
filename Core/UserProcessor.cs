using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    //todo that name is awful...
    public class UserProcessor
    {
        private ISender _sender;

        private IUserService _userService { get; set; }

        public UserProcessor(IUserService userService, ISender sender)
        {
            _userService = userService;
            _sender = sender;
        }

        public async Task<IEnumerable<string>> ProcessAsync()
        {
            var processedUrls = new List<string>();
            var processTasks = new List<Task<IList<string>>>();

            do
            {
                var userBatch = await _userService.GetUserBatch();

                if (userBatch.Any())
                {
                    processTasks.Add(ProcessBatchAsync(userBatch));
                }
            } while (_userService.GetContinuationToken() != null);

            var processTaskResults = await Task.WhenAll(processTasks);

            return AggregateResults(processTaskResults);
        }

        private static List<string> AggregateResults(ICollection<string>[] senderResults)
        {
            var aggregatedUrls = new List<string>();

            foreach (var urlsArray in senderResults)
            {
                aggregatedUrls.AddRange(urlsArray);
            }

            return aggregatedUrls;
        }

        private async Task<IList<string>> ProcessBatchAsync(IEnumerable<User> users)
        {
            var senderTasks = users.Select(x => ProcessUserAsync(x));
            var senderResults = await Task.WhenAll(senderTasks);
            return AggregateResults(senderResults);
        }

        private async Task<IList<string>> ProcessUserAsync(User user)
        {
            var senderTask = _sender.SendAsync(user);
            var userUpdaterTask = _userService.UpdateLastProcessingDateAsync(user);

            var sentUrls = await senderTask;
            await userUpdaterTask;

            return sentUrls.ToList();
        }
    }
}