using Core.EmailSenders;
using PocketSharp;
using PocketSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public interface ISender
    {
        Task SendArticlesAsync(User user);
    }

    public class ArticleSender : ISender
    {
        private const int ArticlesAmount = 10;
        private IEmailSender _emailSender;
        private IParser _parser;
        private IPocketClient _pocketClient;

        public ArticleSender(
            IPocketClient pocketClient,
            IParser parser,
            IEmailSender emailSender)
        {
            _pocketClient = pocketClient;
            _parser = parser;
            _emailSender = emailSender;
        }

        public async Task SendArticlesAsync(User user)
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

                if (parsedArticle != null)
                {
                    await _emailSender.SendEmailWithHtmlAttachmentAsync(
                        user.KindleEmail,
                        parsedArticle.Title,
                        parsedArticle.Content);
                }
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