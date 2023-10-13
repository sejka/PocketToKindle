using Core;
using Core.EmailSenders;
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
            //var config = new ConfigBuilder(".").Build();

            //var parser = new MercuryApiParser("https://mercury-parser.azurewebsites.net/api/MercuryParser?url=");
            //var article = await parser.ParseAsync("https://earth.stanford.edu/news/how-much-does-air-pollution-cost-us#gs.9a641g");

            var apiKey = "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";
            var sender = new SendgridSender(apiKey, "kindle@mail.p2k.sejka.pl");
            await sender.SendEmailWithHtmlAttachmentAsync("teherty@gmail.com", "elo", "beka");
        } 
    }
}