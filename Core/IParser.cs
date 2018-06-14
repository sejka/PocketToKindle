using System.Threading.Tasks;

namespace Core
{
    public interface IParser
    {
        Task<IArticle> ParseAsync(string url);
    }
}