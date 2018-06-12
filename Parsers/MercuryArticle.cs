using Newtonsoft.Json;
using System;

namespace PocketToKindle.Parsers
{
    public class MercuryArticle
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("date_published")]
        public DateTime? DatePublished { get; set; }

        [JsonProperty("lead_image_url")]
        public string LeadImageUrl { get; set; }

        [JsonProperty("dek")]
        public string Dek { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("word_count")]
        public int WordCount { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("rendered_pages")]
        public int RenderedPages { get; set; }

        //todo find other place for this method
        public void AddReportLink(string domain, string functionKey)
        {
            Content = string.Concat(Content, $"<br><a href=\"http://{domain}/api/report?url={Url}&code={functionKey}\">Report this article as incorrectly parsed</a>");
        }
    }
}