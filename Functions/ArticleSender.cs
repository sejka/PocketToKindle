using Core.EmailSenders;
using Microsoft.Extensions.Logging;
using PocketSharp;
using PocketSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace Core
{
    public interface ISender
    {
        Task SendArticlesAsync(User user);
    }

    public class ArticleSender : ISender
    {
        private const int ARTICLES_TO_SEND_LIMIT = 10;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly string _serviceDomain;
        private readonly IParser _parser;
        private readonly IPocketClient _pocketClient;

        public ArticleSender(
            IPocketClient pocketClient,
            IParser parser,
            IEmailSender emailSender,
            IUserService userService,
            ILogger logger,
            string serviceDomain)
        {
            _pocketClient = pocketClient;
            _parser = parser;
            _emailSender = emailSender;
            _userService = userService;
            _logger = logger;
            _serviceDomain = serviceDomain;
        }

        public async Task SendArticlesAsync(User user)
        {
            IEnumerable<PocketItem> allArticles = new List<PocketItem>();
            try
            {
                allArticles = (await GetLastArticlesSinceLastProcessingDate(user, ARTICLES_TO_SEND_LIMIT))
                                .Where(x => !string.IsNullOrEmpty(x.Uri.ToString()));
            }
            catch (PocketException ex)
            {
                //"A valid access token is required to access the requested API endpoint."
                if (ex.PocketErrorCode == 107)
                {
                    await _userService.RemoveUserAsync(user);
                    _logger.LogError($"couldn't get user's data: {user.PocketUsername}, exception: {ex.Message}");
                }
                return;
            }

            if (!allArticles.Any())
            {
                return;
            }

            foreach (var article in allArticles)
            {
                IArticle parsedArticle = null;
                try
                {
                    parsedArticle = await _parser.ParseAsync(article.Uri.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse article: {article.Uri.ToString()}", ex);
                    continue;
                }

                if (parsedArticle != null)
                {
                    try
                    {
                        AddInterfaceLinks(parsedArticle, article.ID, user.Token);
                        await _emailSender.SendEmailWithHtmlAttachmentAsync(
                            user.KindleEmail,
                            parsedArticle.Title.Replace(".", ""),
                            parsedArticle.Content);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to send article", ex);
                    }
                }
            }
        }

        //todo migrate to razor
        private void AddInterfaceLinks(IArticle parsedArticle, string id, string token)
        {
            var interfaceLinksHtml = string.Join("", $"<br><a href=\"http://api.{_serviceDomain}/api/report?url={HttpUtility.UrlEncode(parsedArticle.Url)}\">Report</a><br>",
                $"<a href=\"http://api.{_serviceDomain}/api/archive?articleId={id}&token={token}\">Archive</a><br>",
                $"<a href=\"http://api.{_serviceDomain}/api/star?articleId={id}&token={token}\">Star</a><br>");

            parsedArticle.Content = $"<html><body><h1>{parsedArticle.Title}</h1><h3>{parsedArticle.DatePublished}</h3>{parsedArticle.Content}{interfaceLinksHtml}</body></html>";
        }

        private async Task<IEnumerable<PocketItem>> GetLastArticlesSinceLastProcessingDate(User user, int articlesAmount)
        {
            _pocketClient.AccessCode = user.AccessCode;

            return await _pocketClient.Get(
                state: State.unread,
                contentType: ContentType.article,
                sort: Sort.newest,
                since: user.LastProcessingDate,
                count: articlesAmount);
        }
    }
}