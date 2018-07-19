using Core.EmailSenders;
using PocketSharp;
using PocketSharp.Models;
using System;
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
        private string _serviceDomain;
        private IParser _parser;
        private IPocketClient _pocketClient;

        public ArticleSender(
            IPocketClient pocketClient,
            IParser parser,
            IEmailSender emailSender,
            string serviceDomain)
        {
            _pocketClient = pocketClient;
            _parser = parser;
            _emailSender = emailSender;
            _serviceDomain = serviceDomain;
        }

        public async Task SendArticlesAsync(User user)
        {
            var allArticles = (await GetLastArticlesSinceLastProcessingDate(user, ArticlesAmount)).Where(x => x.Uri != null);

            if (!allArticles.Any())
            {
                return;
            }

            foreach (var article in allArticles)
            {
                var parsedArticle = await _parser.ParseAsync(article.Uri.ToString());

                if (parsedArticle != null)
                {
                    AddInterfaceLinks(parsedArticle, article.ID, user.Token);
                    await _emailSender.SendEmailWithHtmlAttachmentAsync(
                        user.KindleEmail,
                        parsedArticle.Title.Replace(".", ""),
                        parsedArticle.Content);
                }
            }
        }

        private void AddInterfaceLinks(IArticle parsedArticle, string id, string token)
        {
            var interfaceLinksHtml = $"<br><a href=\"https://{_serviceDomain}/api/report?url={parsedArticle.Url}\">Report</a><br>" +
                $"<a href=\"https://{_serviceDomain}/api/archive?articleId={id}&token={token}\">Archive</a><br>" +
                $"<a href=\"https://{_serviceDomain}/api/star?articleId={id}&token={token}\">Star</a><br>";
            parsedArticle.Content = string.Concat(parsedArticle.Content, interfaceLinksHtml);
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