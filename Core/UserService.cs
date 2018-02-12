using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUserBatch();

        TableContinuationToken GetContinuationToken();

        void AddUser(User user);

        Task UpdateLastProcessingDateAsync(User user);
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

        public static UserService BuildUserService(string storageConnectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable userCloudTable = tableClient.GetTableReference("users");
            userCloudTable.CreateIfNotExistsAsync();
            var userService = new UserService(userCloudTable);
            return userService;
        }

        public async void AddUser(User user)
        {
            TableOperation insertOperation = TableOperation.Insert(user);
            user.PartitionKey = user.PocketUsername.Substring(0, Math.Min(user.PocketUsername.Length, 3));
            user.RowKey = user.PocketUsername;
            var result = await _userTable.ExecuteAsync(insertOperation);
        }

        public async Task UpdateLastProcessingDateAsync(User user)
        {
            user.LastProcessingDate = DateTime.UtcNow;
            TableOperation updateOperation = TableOperation.Replace(user);
            var result = await _userTable.ExecuteAsync(updateOperation);
        }
    }
}