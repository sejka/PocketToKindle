using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class BrevoSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly ILogger<BrevoSender> _logger;

        public BrevoSender(IHttpClientFactory factory, string apiKey, string senderEmail, ILogger<BrevoSender> logger)
        {
            _httpClient = factory.CreateClient();
            _apiKey = apiKey;
            _senderEmail = senderEmail;
            _logger = logger;
        }

        public async Task SendEmailWithHtmlAttachmentAsync(string email, string title, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent("{\"sender\": {\"name\": \"Pocket 2 Kindle\", \"email\": \"" + _senderEmail + "\"},  \"attachment\": [{\"content\": \"" + Base64Encode(content) + "\", \"name\": \"" + title.Normalize() + ".html\"}],  \"to\": [{\"email\": \"" + email + "\"}],  \"subject\": \"Login Email confirmation\",  \"textContent\": \"Login Email confirmation\"}");
            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"failed to send an email {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"failed to send an email: {ex.Message}");
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}