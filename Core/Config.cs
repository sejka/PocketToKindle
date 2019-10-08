using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Core
{
    public class Config
    {
        public string StorageConnectionString { get; set; }
        public string PocketConsumerKey { get; set; }
        public string PocketRedirectUri { get; set; }
        public SmtpSenderOptions EmailSenderOptions { get; set; }
        public MailgunSenderOptions MailGunSenderOptions { get; set; }
        public string ServiceDomain { get; set; }
        public string MercuryApiParserEndpoint { get; set; }
    }

    public class SmtpSenderOptions
    {
        public string Host { get; internal set; }
        public int Port { get; internal set; }
        public string Login { get; internal set; }
        public string Password { get; internal set; }
    }

    public class MailgunSenderOptions
    {
        public string ApiKey { get; internal set; }
        public string HostEmail { get; internal set; }
    }

    public class ConfigBuilder
    {
        private IConfigValuesProvider configValuesProvider { get; set; } = new EnvironmentValuesProvider();

        public ConfigBuilder(string functionAppDirectory)
        {
            if (Environment.GetEnvironmentVariable("P2K_IS_PRODUCTION") != "true")
            {
                configValuesProvider = new JsonFileValuesProvider(functionAppDirectory);
            }
        }

        public Config Build()
        {
            return new Config
            {
                StorageConnectionString = configValuesProvider.Get("STORAGE_CONNECTION_STRING"),
                PocketConsumerKey = configValuesProvider.Get("POCKET_CONSUMER_KEY"),
                PocketRedirectUri = configValuesProvider.Get("POCKET_REDIRECT_URI"),
                ServiceDomain = configValuesProvider.Get("SERVICE_DOMAIN"),

                //EmailSenderOptions = new SmtpSenderOptions
                //{
                //    Host = configValuesProvider.Get("EMAIL:HOST"),
                //    Login = configValuesProvider.Get("EMAIL:LOGIN"),
                //    Password = configValuesProvider.Get("EMAIL:PASSWORD"),
                //    Port = Convert.ToInt32(configValuesProvider.Get("EMAIL:PORT"))
                //},
                MailGunSenderOptions = new MailgunSenderOptions
                {
                    ApiKey = configValuesProvider.Get("MAILGUN:APIKEY"),
                    HostEmail = configValuesProvider.Get("MAILGUN:HOSTEMAIL")
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
            return Environment.GetEnvironmentVariable(valueName);
        }
    }

    internal class JsonFileValuesProvider : IConfigValuesProvider
    {
        private IConfigurationRoot _config;

        public JsonFileValuesProvider(string functionAppDirectory)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(functionAppDirectory, "config.json"))
                .AddUserSecrets<Config>()
                .Build();
        }

        public string Get(string valueName)
        {
            return _config[valueName];
        }
    }
}