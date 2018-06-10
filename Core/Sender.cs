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
        Task SendAsync(User user);
    }

    public class Sender : ISender
    {
        private const int ArticlesAmount = 5;
        private IEmailSender _emailSender;
        private IParser _parser;
        private IPocketClient _pocketClient;

        public Sender(
            IPocketClient pocketClient,
            IParser parser,
            IEmailSender emailSender)
        {
            _pocketClient = pocketClient;
            _parser = parser;
            _emailSender = emailSender;
        }

        public async Task SendAsync(User user)
        {
            var allArticles = (await GetLastArticlesSinceLastProcessingDate(user, ArticlesAmount)).Where(x => x.Uri != null);

            if (!allArticles.Any())
            {
                return;
            }

            var allArticleUrls = allArticles.Select(article => article.Uri.ToString());

            foreach (var articleUrl in allArticleUrls)
            {
                var parsedArticle = await _parser.ParseAsync(articleUrl);

                await _emailSender.SendEmailWithHtmlAttachmentAsync(user.KindleEmail, parsedArticle.Title, $@"<html><body>{parsedArticle.Content}</body></html>");
            }
        }

        private Task<IEnumerable<PocketItem>> GetLastArticlesSinceLastProcessingDate(User user, int articlesAmount)
        {
            _pocketClient.AccessCode = user.AccessCode;

            return _pocketClient.Get(
                contentType: ContentType.article,
                sort: Sort.newest,
                since: user.LastProcessingDate,
                count: articlesAmount);
        }
    }
}