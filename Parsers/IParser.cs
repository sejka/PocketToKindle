using PocketToKindle.Models;
using System.Threading.Tasks;

namespace PocketToKindle.Parsers
{
    public interface IParser
    {
        Task<MercuryArticle> ParseAsync(string url);
    }
}