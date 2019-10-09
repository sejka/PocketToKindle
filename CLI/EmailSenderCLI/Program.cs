using Core;
using Parsers;
using PocketSharp;
using System;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal static class Program
    {
        private static async Task Main()
        {
            var config = new ConfigBuilder(".").Build();

            var parser = new MercuryApiParser(config.MercuryParserApiEndpoint);
            var mailgunSender = new Core.EmailSenders.MailgunSender(config.MailgunApiKey, config.MailgunHostEmail);
            var articleSender = new ArticleSender(new PocketClient(config.PocketConsumerKey, "364b995b-878a-a0f8-f959-972e07"),
                                                  parser,
                                                  mailgunSender,
                                                  "p2k.sejka.pl");
            await articleSender.SendArticlesAsync(
                new User
                {
                    AccessCode = "364b995b-878a-a0f8-f959-972e07",
                    KindleEmail = "teherty@gmail.com",
                    LastProcessingDate = DateTime.UtcNow.AddMonths(-1)
                });
        }
    }
}