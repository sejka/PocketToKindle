using Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RegisterCLI
{
    internal class Program
    {
        private static Config _config = new ConfigBuilder(".").Build();
        private static HttpClient _httpClient = new HttpClient();

        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var registerRequest = new { consumer_key = _config.PocketConsumerKey, redirect_uri = _config.PocketConsumerKey };
            var content = JsonConvert.SerializeObject(registerRequest);
            Console.WriteLine(content);
            var requestKeyTask = await _httpClient.PostAsync($"https://getpocket.com/v3/oauth/request", new StringContent(content, Encoding.UTF8, "application/json"));
            var requestKeyResponse = await requestKeyTask.Content.ReadAsStringAsync();
            var requestKey = requestKeyResponse.Split('=')[1];
            Console.WriteLine($"Got response with request key: {requestKey}");

            var redirectUrl = $"https://getpocket.com/auth/authorize?request_token={requestKey}&redirect_uri={_config.PocketRedirectUri}";
            OpenBrowser(redirectUrl);
            Console.WriteLine("Press any key after completing registration...");
            Console.ReadLine();

            var pocketAccessTokenRequest = new { consumer_key = _config.PocketConsumerKey, code = requestKey };
            var pocketAccessTokenRequestJson = JsonConvert.SerializeObject(pocketAccessTokenRequest);
            var accessTokenResponse = await _httpClient.PostAsync($"https://getpocket.com/v3/oauth/authorize", new StringContent(pocketAccessTokenRequestJson, Encoding.UTF8, "application/json"));

            var accessTokenString = await accessTokenResponse.Content.ReadAsStringAsync();
            var creds = accessTokenString.Split('&');
            var accessToken = creds[0].Split('=')[1];
            var username = creds[1].Split('=')[1];
            Console.WriteLine($"access token: {accessToken}, username: {username}");

            IUserService userService = UserService.BuildUserService(_config.StorageConnectionString);

            Console.WriteLine("Input your kindle email:");
            var kindleEmail = Console.ReadLine();

            var user = new User()
            {
                AccessCode = accessToken,
                PocketUsername = username,
                KindleEmail = kindleEmail,
                LastProcessingDate = DateTime.UtcNow
            };

            userService.AddUser(user);

            Console.WriteLine("Bye World!");
            Console.ReadLine();

            return 0;
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Console.WriteLine($"Please visit: {url}");
            }
        }
    }
}