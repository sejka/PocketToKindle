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
            Console.WriteLine(_client.GenerateRegistrationUri(requestCode).ToString());
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
    }
}