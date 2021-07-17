using Microsoft.Extensions.Configuration;
using System.IO;

namespace Core
{
    public class Config
    {
        public string StorageConnectionString { get; set; }
        public string PocketConsumerKey { get; set; }
        public string PocketRedirectUri { get; set; }
        public string SendgridApiKey { get; set; }
        public string MailgunHostEmail { get; set; }
        public string ServiceDomain { get; set; }
        public string MercuryParserApiEndpoint { get; set; }
    }

    public class ConfigBuilder
    {
        private readonly string _jsonConfigPath;

        public ConfigBuilder(string functionAppDirectory)
        {
            _jsonConfigPath = Path.Combine(functionAppDirectory, "config.json");
        }

        public Config Build()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var config = new Config();

            var configRoot = configurationBuilder
                .AddJsonFile(_jsonConfigPath)
                .AddEnvironmentVariables()
                .AddUserSecrets<Config>()
                .Build();

            configRoot.Bind(config);

            return config;
        }
    }
}