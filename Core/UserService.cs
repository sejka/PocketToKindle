using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUserBatch();

        TableContinuationToken GetContinuationToken();
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
    }
}