using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUserBatch();

        TableContinuationToken GetContinuationToken();

        Task AddUserAsync(User user);

        Task UpdateLastProcessingDateAsync(User user);

        Task<User> FindUserWithToken(string token);
    }

    //todo should i test this?
    public class UserService : IUserService
    {
        private readonly TableQuery<User> allUsersQuery = new TableQuery<User>();
        private TableContinuationToken _continuationToken;
        private CloudTable _userTable;

        public UserService(CloudTable userTable)
        {
            _userTable = userTable;
        }

        public async Task<User> FindUserWithToken(string token)
        {
            var query = new TableQuery<User>().Where(TableQuery.GenerateFilterCondition("Token", QueryComparisons.Equal, token));
            var queryResult = await _userTable.ExecuteQuerySegmentedAsync(query, _continuationToken);

            return queryResult.Results.FirstOrDefault();
        }

        public async Task<IEnumerable<User>> GetUserBatch()
        {
            var queryResult = await _userTable.ExecuteQuerySegmentedAsync(allUsersQuery, _continuationToken);
            _continuationToken = queryResult.ContinuationToken;

            return queryResult.Results;
        }

        public TableContinuationToken GetContinuationToken()
        {
            return _continuationToken;
        }

        public static IUserService BuildUserService(string storageConnectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable userCloudTable = tableClient.GetTableReference("users");
            return new UserService(userCloudTable);
        }

        public async Task AddUserAsync(User user)
        {
            user.KindleEmail.Trim();
            TableOperation insertOperation = TableOperation.Insert(user);
            user.PartitionKey = "user";
            user.RowKey = user.PocketUsername;
            var result = await _userTable.ExecuteAsync(insertOperation);
        }

        public async Task UpdateLastProcessingDateAsync(User user)
        {
            user.LastProcessingDate = DateTime.UtcNow;
            TableOperation updateOperation = TableOperation.Replace(user);
            var result = await _userTable.ExecuteAsync(updateOperation);

            if (result.HttpStatusCode != 200)
            {
            }
        }
    }
}