﻿using Core.EmailSenders;
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
        private readonly IEmailSender _emailSender;
        private readonly string _serviceDomain;
        private readonly IParser _parser;
        private readonly IPocketClient _pocketClient;

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
            var allArticles = (await GetLastArticlesSinceLastProcessingDate(user, ArticlesAmount))
                                .Where(x => !string.IsNullOrEmpty(x.Uri.ToString()));

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

        //todo migrate to razor
        private void AddInterfaceLinks(IArticle parsedArticle, string id, string token)
        {
            var interfaceLinksHtml = string.Join("", $"<br><a href=\"https://{_serviceDomain}/api/report?url={parsedArticle.Url}\">Report</a><br>",
                $"<a href=\"https://{_serviceDomain}/api/archive?articleId={id}&token={token}\">Archive</a><br>",
                $"<a href=\"https://{_serviceDomain}/api/star?articleId={id}&token={token}\">Star</a><br>");

            parsedArticle.Content = $"<html><body><h1>{parsedArticle.Title}</h1><h3>{parsedArticle.DatePublished}</h3>{parsedArticle.Content}{interfaceLinksHtml}</body></html>";
        }

        private async Task<IEnumerable<PocketItem>> GetLastArticlesSinceLastProcessingDate(User user, int articlesAmount)
        {
            _pocketClient.AccessCode = user.AccessCode;

            try
            {
                return await _pocketClient.Get(
                    state: State.unread,
                    contentType: ContentType.article,
                    sort: Sort.newest,
                    since: user.LastProcessingDate,
                    count: articlesAmount);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}