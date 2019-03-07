using Core;
using Core.EmailSenders;
using Moq;
using Parsers;
using PocketSharp;
using PocketSharp.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal static class Program
    {
        private static async Task Main()
        {
            var config = new ConfigBuilder(".").Build();
            var parser = new ReadSharpParser();
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

            var sender = new ArticleSender(pocketClientMock.Object, parser, emailSender, config.ServiceDomain);
            await sender.SendArticlesAsync(new User { KindleEmail = "teherty@gmail.com", Token = "testtoken" });
        }
    }
}