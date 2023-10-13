using System.Threading.Tasks;

namespace Core
{
    public interface IQueue<T>
    {
        Task QueueUserAsync(T user);
    }
}