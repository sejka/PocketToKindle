using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    //todo should i test this?
    public class SmtpSender : IEmailSender
    {
        private SmtpSenderOptions _options;
        private SmtpClient _smtpClient;

        public SmtpSender(SmtpSenderOptions options)
        {
            _options = options;
            _smtpClient = new SmtpClient();
        }

        public void Connect()
        {
            _smtpClient.Connect(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
            _smtpClient.AuthenticateAsync(_options.Login, _options.Password);
        }

        public async Task SendEmailWithHtmlAttachmentAsync(string email, string title, string content)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pocket to Kindle", "pockettokindle@gmail.com"));
            message.To.Add(new MailboxAddress(email));
            message.Subject = title;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = content
            };

            try
            {
                Connect();
                await _smtpClient.SendAsync(message);
                Console.WriteLine($"Successfully sent email: {message.Subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sending message failed: {ex.Message}");
            }
            finally
            {
                await _smtpClient.DisconnectAsync(true);
            }
        }
    }
}