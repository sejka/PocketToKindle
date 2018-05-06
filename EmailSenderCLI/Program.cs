using Core;
using Core.EmailSenders;
using PocketToKindle.Parsers;
using System;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config = new ConfigBuilder(".").Build();
            var content = await new MercuryParser(config.MercuryApiKey, config.ServiceDomain, config.FunctionKey).ParseAsync("https://blog.wikimedia.org/2018/04/20/why-it-took-a-long-time-to-build-that-tiny-link-preview-on-wikipedia/");
            var mailgunSender = new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail);
            await mailgunSender.SendEmailWithHtmlAttachmentAsync("teherty_31b450@kindle.com", content.Title, $"<html><head><title>{content.Title}</title></head><body>{content.Content}</body></html>");
        }
    }
}