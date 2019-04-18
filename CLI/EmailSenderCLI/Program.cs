using Core;
using Core.EmailSenders;
using System.Threading.Tasks;

namespace EmailSenderCLI
{
    internal static class Program
    {
        private static async Task Main()
        {
            var config = new ConfigBuilder(".").Build();

            var mailgunSender = new MailgunSender(config.MailGunSenderOptions.ApiKey, config.MailGunSenderOptions.HostEmail);
            await mailgunSender.SendEmailWithHtmlAttachmentAsync(
                "teherty@gmail.com",
                "some test subject",
                @"<html><body><h1>test content</h1></body></html>");
        }
    }
}