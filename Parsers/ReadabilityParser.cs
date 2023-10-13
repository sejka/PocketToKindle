using Core;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Parsers
{
    public class ReadabilityParser : IParser
    {
        private readonly HttpClient _httpClient;
        private readonly ImageInliner _imageInliner;
        private readonly string _apiEndpointUrl;

        public ReadabilityParser(string apiEndpointUrl, IHttpClientFactory factory, ImageInliner imageInliner)
        {
            _apiEndpointUrl = apiEndpointUrl;
            _httpClient = factory.CreateClient();
            _imageInliner = imageInliner;
        }

        public async Task<IArticle> ParseAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }

            if (url.Contains("twitter.com"))
            {
                url = $"https://nitter.net{new Uri(url).AbsolutePath}";
            }

            var result = await _httpClient.GetAsync(string.Concat(_apiEndpointUrl, "/readability?url=", url));

            if (result.IsSuccessStatusCode)
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new CustomJsonConverterForNullableDateTime());
                string json = await result.Content.ReadAsStringAsync();
                var article = JsonSerializer.Deserialize<ReadabilityArticle>(json, options);
                if (article.Content == null)
                {
                    throw new Exception("failed to parse article, no content");
                }

                await _imageInliner.InlineImagesAsync(article, url);
                return article;
            }
            else
            {
                throw new Exception($"Failed to parse article {url}, {result.Content.ReadAsStringAsync()}");
            }
        }
    }

    public class ReadabilityArticle : IArticle
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("byline")]
        public string Author { get; set; }

        public object dir { get; set; }
        public string textContent { get; set; }
        public string excerpt { get; set; }
        public string siteName { get; set; }
        public int length { get; set; }
        public DateTime? DatePublished { get; set; }
    }

    internal class CustomJsonConverterForNullableDateTime : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return reader.GetString() == "" ? null : reader.GetDateTime();
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        // This method will be ignored on serialization, and the default typeof(DateTime) converter is used instead.
        // This is a bug: https://github.com/dotnet/corefx/issues/41070#issuecomment-560949493
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            Console.WriteLine("Here - writing");

            if (!value.HasValue)
            {
                writer.WriteStringValue("");
            }
            else
            {
                writer.WriteStringValue(value.Value);
            }
        }
    }
}