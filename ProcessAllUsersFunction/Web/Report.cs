using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Functions.Web
{
    public static class Report
    {
        [FunctionName("Report")]
        [return: Table("ReportedUrls")]
        public static ReportedUrl Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            Uri uriResult;
            bool isValidUri = Uri.TryCreate(req.Query["url"], UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return isValidUri ? new ReportedUrl
            {
                PartitionKey = "reportedUrl",
                RowKey = Guid.NewGuid().ToString(),
                Url = uriResult.AbsoluteUri
            } : null;
        }

        public class ReportedUrl : TableEntity
        {
            public string Url { get; set; }
        }
    }
}