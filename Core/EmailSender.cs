using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Core
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string title, string content);
    }

    //todo should i test this?
    public class EmailSender : IEmailSender
    {
        private EmailSenderOptions _options;
        private SmtpClient _smtpClient;

        public EmailSender(EmailSenderOptions options)
        {
            _options = options;
            _smtpClient = new SmtpClient();
        }

        public void Connect()
        {
            _smtpClient.Connect(_options.Host);
            _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            _smtpClient.AuthenticateAsync(_options.Login, _options.Password);
        }

        public async Task SendEmailAsync(string email, string title, string content)
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
            }
            finally
            {
                await _smtpClient.DisconnectAsync(true);
            }
        }
    }
}