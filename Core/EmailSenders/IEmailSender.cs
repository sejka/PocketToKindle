using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public interface IEmailSender
    {
        Task SendEmailWithHtmlAttachmentAsync(string email, string title, string content);
    }
}