using Core;
using Newtonsoft.Json;
using PocketSharp;
using PocketSharp.Models;
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

            var _client = new PocketClient(_config.PocketConsumerKey, callbackUri: _config.PocketRedirectUri);
            string requestCode = await _client.GetRequestCode();
            OpenBrowser(_client.GenerateRegistrationUri(requestCode).ToString());
            PocketUser pocketUser = await _client.GetUser(requestCode);

            IUserService userService = UserService.BuildUserService(_config.StorageConnectionString);

            Console.WriteLine("Input your kindle email:");
            var kindleEmail = Console.ReadLine();

            var user = new User()
            {
                AccessCode = pocketUser.Code,
                PocketUsername = pocketUser.Username,
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