using Core;
using PocketToKindle.Parsers;
using Xunit;

namespace PocketToKindleTests
{
    public class MercuryParserTests
    {
        private Config _config = new ConfigBuilder(".").Build();

        [Fact]
        public async void Parser_ParsesCorrectlySampleArticle()
        {
            string testUrl = "https://waitbutwhy.com/2015/01/artificial-intelligence-revolution-1.html";
            MercuryParser mercuryParser = new MercuryParser(_config.MercuryApiKey, _config.ServiceDomain);

            var article = await mercuryParser.ParseAsync(testUrl);

            Assert.Equal("The Artificial Intelligence Revolution: Part 1", article.Title);
        }
    }
}