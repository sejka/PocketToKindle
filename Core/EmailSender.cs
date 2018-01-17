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
        private SmtpClient _smtpClient;

        public EmailSender(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
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

            //todo i think it'll be problematic when sharing smtpClient :P
            await _smtpClient.SendAsync(message);
        }
    }
}