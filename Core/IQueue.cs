using System.Threading.Tasks;

namespace Core
{
    public interface IQueue
    {
        Task QueueUserAsync(User user);
    }
}