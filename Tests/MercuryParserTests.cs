using Core;
using Parsers;
using System;
using Xunit;

namespace Tests
{
    public class MercuryParserTests
    {
        private readonly Config _config = new ConfigBuilder(".").Build();

        [Fact]
        public async void ParsesCorrectlySampleArticle()
        {
            string testUrl = "https://waitbutwhy.com/2015/01/artificial-intelligence-revolution-1.html";
            MercuryParser mercuryParser = new MercuryParser(_config.MercuryApiKey);

            var article = await mercuryParser.ParseAsync(testUrl);

            Assert.Equal("The Artificial Intelligence Revolution: Part 1", article.Title);
        }

        [Fact]
        public async void DoesNotThrowOnInvalidArticleUrl()
        {
            string testUrl = "http://fake.website.com/article";

            MercuryParser mercuryParser = new MercuryParser(_config.MercuryApiKey);

            var article = await mercuryParser.ParseAsync(testUrl);
        }

        [Fact]
        public async void ThrowsOnEmptyUrl()
        {
            string testUrl = "";

            var mercuryParser = new MercuryParser(_config.MercuryApiKey);

            await Assert.ThrowsAsync<ArgumentException>(() => mercuryParser.ParseAsync(testUrl));
        }
    }
}