using Microsoft.Extensions.Configuration;
using System.IO;

namespace Core
{
    public class Config
    {
        public string StorageConnectionString { get; set; }
        public string PocketConsumerKey { get; set; }
        public string PocketRedirectUri { get; set; }
        public MailgunOptions Mailgun { get; set; } = new MailgunOptions();
        public string ServiceDomain { get; set; }
        public string MercuryParserApiEndpoint { get; set; }
    }

    public class MailgunOptions
    {
        public string ApiKey { get; set; }
        public string HostEmail { get; set; }
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
                .AddJsonFile(_jsonConfigPath, false)
                .AddEnvironmentVariables()
                .AddUserSecrets<Config>()
                .Build();

            configRoot.Bind(config);

            return config;
        }
    }
}