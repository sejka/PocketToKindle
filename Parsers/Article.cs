using System;

namespace PocketToKindle.Models
{
    public class Article
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? Date_published { get; set; }
        public string Lead_image_url { get; set; }
        public string dek { get; set; }
        public string Url { get; set; }
        public string Domain { get; set; }
        public string Excerpt { get; set; }
        public int Word_count { get; set; }
        public string Direction { get; set; }
        public int Total_pages { get; set; }
        public int Rendered_pages { get; set; }
        public object Next_page_url { get; set; }

        public void AddReportLink(string domain)
        {
            //todo find other place for this method
            string.Concat(Content, $"<a href=\"{domain}/api/report?url={Url}\">Report this article as incorrectly parsed</a>");
        }
    }
}