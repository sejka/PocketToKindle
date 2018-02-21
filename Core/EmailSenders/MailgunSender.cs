using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class MailgunSender : IEmailSender
    {
        private string _apiKey;
        private string _hostEmail;
        private string _domainName;
        private static HttpClient _httpClient = new HttpClient();

        public MailgunSender(string apiKey, string hostEmail)
        {
            _httpClient.BaseAddress = new Uri("https://api.mailgun.net/v3/");
            var authByteArray = Encoding.ASCII.GetBytes($"api:{_apiKey}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authByteArray));
            _hostEmail = hostEmail;
            _domainName = hostEmail.Split('@')[1];
        }

        public async Task SendEmailAsync(string email, string title, string content)
        {
            var parameters = new Dictionary<string, string> {
                { "from", _hostEmail },
                { "to", email },
                { "subject", title },
                { "html", content }
            };
            var args = new FormUrlEncodedContent(parameters);
            await _httpClient.PostAsync("", args);
        }
    }
}