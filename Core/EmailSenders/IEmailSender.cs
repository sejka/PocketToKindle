using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string title, string content);
    }
}