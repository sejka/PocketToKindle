using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class MailgunSender : IEmailSender
    {
        private string _apiKey;
        private string _hostEmail;
        private string _domainName;
        private RestClient _client { get; set; } = new RestClient("https://api.mailgun.net/v3/");

        public MailgunSender(string apiKey, string hostEmail)
        {
            _apiKey = apiKey;
            _hostEmail = hostEmail;
            _domainName = hostEmail.Split('@')[1];
            _client.Authenticator = new HttpBasicAuthenticator("api", _apiKey);
        }

        public async Task SendEmailAsync(string email, string title, string content)
        {
            var request = new RestRequest($"{_domainName}/messages");
            request.AddParameter("from", _hostEmail);
            request.AddParameter("to", email);
            request.AddParameter("subject", title);
            request.AddParameter("html", content);
            request.Method = Method.POST;
            var response = _client.Execute(request);
        }
    }
}