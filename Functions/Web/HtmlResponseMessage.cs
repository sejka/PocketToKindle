using System.Net;
using System.Net.Http;

namespace Functions.Web
{
    public class HtmlResponseMessage : HttpResponseMessage
    {
        public HtmlResponseMessage(HttpStatusCode code, string html)
        {
            this.StatusCode = code;
            this.Content = new StringContent(html);
            this.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
        }
    }
}