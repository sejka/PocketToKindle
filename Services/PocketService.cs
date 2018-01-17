using Newtonsoft.Json;
using PocketToKindle.Services.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PocketToKindle.Services
{
    public class PocketService
    {
        private HttpClient _httpClient = new HttpClient();
        private string _consumerKey;
        private string _redirectUrl;

        public PocketService(string consumerKey, string redirectUrl)
        {
            _httpClient.BaseAddress = new Uri("https://getpocket.com/v3/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("X-Accept", "application/json");
            _consumerKey = consumerKey;
            _redirectUrl = redirectUrl;
        }

        public async Task<string> GetAuthorizationUrl()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("consumer_key", _consumerKey);
            parameters.Add("redirect_uri", _redirectUrl);
            FormUrlEncodedContent data = new FormUrlEncodedContent(parameters);
            var responseMessage = await _httpClient.PostAsync("oauth/request", data);

            var apiRequestToken = JsonConvert.DeserializeObject<TokenResponse>(responseMessage.ToString()).Code;

            _httpClient.DefaultRequestHeaders.Add("request_token", apiRequestToken);

            return apiRequestToken;
        }

        public async Task<UserAccessTokenResponse> ConvertRequestTokenToUserAccessToken(string requestToken)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("consumer_key", _consumerKey);
            parameters.Add("code", requestToken);
            FormUrlEncodedContent data = new FormUrlEncodedContent(parameters);
            var responseMessage = await _httpClient.PostAsync("oauth/authorize", null);

            var userAccessToken = JsonConvert.DeserializeObject<UserAccessTokenResponse>(responseMessage.ToString());

            return userAccessToken;
        }
    }
}