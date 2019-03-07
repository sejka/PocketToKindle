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
            var readSharpParser = new ReadSharpParser();

            var article = await readSharpParser.ParseAsync(testUrl);

            Assert.Equal("The Artificial Intelligence Revolution: Part 1", article.Title);
        }

        [Fact]
        public async void DoesNotThrowOnInvalidArticleUrl()
        {
            string testUrl = "http://fake.website.com/article";

            var readSharpParser = new ReadSharpParser();

            var article = await readSharpParser.ParseAsync(testUrl);
        }

        [Fact]
        public async void ThrowsOnEmptyUrl()
        {
            string testUrl = "";

            var readSharpParser = new ReadSharpParser();

            await Assert.ThrowsAsync<ArgumentException>(() => readSharpParser.ParseAsync(testUrl));
        }
    }
}