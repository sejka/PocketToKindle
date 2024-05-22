using Microsoft.Extensions.Configuration;
using System.IO;

namespace Core
{
    public class Config
    {
        public string PocketConsumerKey { get; set; }
        public string PocketRedirectUri { get; set; }
        public string EmailSenderApiKey { get; set; }
        public string HostEmail { get; set; }
        public string ServiceDomain { get; set; }
        public string ParsersApiEndpoint { get; set; }
        public string whatever { get; set; }
    }

    public class ConfigBuilder
    {
        private readonly string _jsonConfigPath;

        public ConfigBuilder(string functionAppDirectory)
        {
            _jsonConfigPath = Path.Combine(functionAppDirectory, "appsettings.json");
        }

        public Config Build()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var config = new Config();

            var configRoot = configurationBuilder
                .AddJsonFile(_jsonConfigPath, true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Config>(true)
                .Build();

            configRoot.Bind(config);

            return config;
        }
    }
}
