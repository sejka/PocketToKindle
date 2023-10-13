using Microsoft.EntityFrameworkCore;

namespace Web.Database
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();

        Task AddUserAsync(User user);

        Task UpdateLastProcessingDateAsync(User user);

        Task<User> FindUserWithToken(string token);

        Task RemoveUserAsync(string username);
    }

    public class UserService : IUserService
    {
        private readonly P2kDbContext _context;

        public UserService(P2kDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public Task<User> FindUserWithToken(string token)
        {
            return _context.Users.SingleAsync(u => u.Token == token);
        }

        public Task<List<User>> GetUsers()
        {
            return _context.Users.ToListAsync();
        }

        public async Task RemoveUserAsync(string username)
        {
            var user = await _context.Users.SingleAsync(x => x.PocketUsername == username);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastProcessingDateAsync(User user)
        {
            var usr = await _context.Users.SingleAsync(x => x.PocketUsername == user.PocketUsername);
            usr.LastProcessingDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}