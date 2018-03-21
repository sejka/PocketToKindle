using Core.EmailSenders;
using PocketSharp;
using PocketSharp.Models;
using PocketToKindle.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public interface ISender
    {
        Task<ICollection<string>> SendAsync(User user);
    }

    public class Sender : ISender
    {
        private IEmailSender _emailSender;
        private IParser _parser;
        private IPocketClient _pocketClient;

        public Sender(IPocketClient pocketClient, IParser parser, IEmailSender emailSender)
        {
            _pocketClient = pocketClient;
            _parser = parser;
            _emailSender = emailSender;
        }

        public async Task<ICollection<string>> SendAsync(User user)
        {
            var allArticles = (await GetArticlesSince(user)).Where(x => x.Uri != null);

            if (!allArticles.Any())
            {
                return new List<string>();
            }

            var allArticleUrls = allArticles.Select(article => article.Uri.ToString());
            var resultArticles = new List<string>();

            foreach (var articleUrl in allArticleUrls)
            {
                var parsedArticle = await _parser.ParseAsync(articleUrl);
                await _emailSender.SendEmailWithHtmlAttachmentAsync(user.KindleEmail, parsedArticle.Title, $@"<html><body>{parsedArticle.Content}</body></html>");

                resultArticles.Add(articleUrl);
            }

            return resultArticles;
        }

        private async Task<IEnumerable<PocketItem>> GetArticlesSince(User user)
        {
            _pocketClient.AccessCode = user.AccessCode;

            //don't get more than 5 last articles
            var articles = await _pocketClient.Get(since: user.LastProcessingDate, count: 5);

            return articles;
        }
    }
}