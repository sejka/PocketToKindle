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
            var content = await new MercuryParser(config.MercuryApiKey).ParseAsync("https://blog.scooletz.com/2017/12/07/azure-functions-processing-2-billions-items-per-day-3/");
            var mailgunSender = new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail);

            await mailgunSender.SendEmailWithHtmlAttachmentAsync("teherty_31b450@kindle.com", content.Title, $@"<html><body><b>{content.Content}</b></body></html>");
        }
    }
}