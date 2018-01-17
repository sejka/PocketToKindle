using Microsoft.Extensions.Configuration;

namespace Core
{
    public class Config
    {
        public string StorageConnectionString { get; set; }
        public string MercuryApiKey { get; set; }
        public string PocketConsumerKey { get; set; }
        public EmailSenderOptions EmailSenderOptions { get; set; }
    }

    public class EmailSenderOptions
    {
        public string Host { get; internal set; }
        public string Port { get; internal set; }
        public string Login { get; internal set; }
        public string Password { get; internal set; }
    }

    public class ConfigBuilder
    {
        private IConfigValuesProvider configValuesProvider { get; set; } = new EnvironmentValuesProvider();

        public ConfigBuilder()
        {
            if (System.Environment.GetEnvironmentVariable("P2K_IS_PRODUCTION") != "true")
            {
                configValuesProvider = new JsonFileValuesProvider();
            }
        }

        public Config Build()
        {
            return new Config
            {
                MercuryApiKey = configValuesProvider.Get("MERCURY_API_KEY"),
                StorageConnectionString = configValuesProvider.Get("STORAGE_CONNECTION_STRING"),
                PocketConsumerKey = configValuesProvider.Get("POCKET_CONSUMER_KEY"),
                EmailSenderOptions = new EmailSenderOptions
                {
                    Host = configValuesProvider.Get("EMAIL:HOSTNAME"),
                    Login = configValuesProvider.Get("EMAIL:LOGIN"),
                    Password = configValuesProvider.Get("EMAIL:PASSWORD"),
                    Port = configValuesProvider.Get("EMAIL:PORT")
                }
            };
        }
    }

    internal interface IConfigValuesProvider
    {
        string Get(string valueName);
    }

    internal class EnvironmentValuesProvider : IConfigValuesProvider
    {
        public string Get(string valueName)
        {
            return System.Environment.GetEnvironmentVariable(valueName);
        }
    }

    internal class JsonFileValuesProvider : IConfigValuesProvider
    {
        private IConfigurationRoot _config = new ConfigurationBuilder()
                                                .AddJsonFile("./config.json")
                                                .Build();

        public string Get(string valueName)
        {
            return _config[valueName];
        }
    }
}