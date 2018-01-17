using PocketToKindle.Models;
using System.Threading.Tasks;

namespace PocketToKindle.Parsers
{
    public interface IParser
    {
        Task<Article> ParseAsync(string url);
    }
}