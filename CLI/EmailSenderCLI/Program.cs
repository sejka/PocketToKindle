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
            var article = await parser.ParseAsync("https://earth.stanford.edu/news/how-much-does-air-pollution-cost-us#gs.9a641g");
        }
    }
}