using Core;
using Core.EmailSenders;
using Moq;
using PocketSharp;
using PocketSharp.Models;
using PocketToKindle.Parsers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var config = new ConfigBuilder(".").Build();
            IParser parser = new MercuryParser(config.MercuryApiKey, config.ServiceDomain, config.FunctionKey);
            var emailSender = new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail);
            var pocketClientMock = new Mock<IPocketClient>();

            pocketClientMock.Setup(x =>
                x.Get(
                    It.IsAny<State>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<Sort>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>())).Returns(
                    Task.FromResult(
                        (IEnumerable<PocketItem>)new PocketItem[] {
                            new PocketItem { Uri = new Uri("https://blog.wikimedia.org/2018/04/20/why-it-took-a-long-time-to-build-that-tiny-link-preview-on-wikipedia/") }
                        }
                    )
                );

            var sender = new Sender(pocketClientMock.Object, parser, emailSender);
            await sender.SendAsync(new User { KindleEmail = "teherty@gmail.com" });
        }
    }
}