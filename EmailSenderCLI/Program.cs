using Core;
using Core.EmailSenders;
using PocketToKindle.Parsers;
using System;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config = new ConfigBuilder(".").Build();
            var content = await new MercuryParser(config.MercuryApiKey).ParseAsync("https://kateheddleston.com/blog/becoming-a-10x-developer");
            var mailgunSender = new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail);

            await mailgunSender.SendEmailWithHtmlAttachmentAsync("teherty_31b450@kindle.com", content.Title, $@"<html><body>{content.Content}</body></html>");
        }
    }
}