using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Core
{
    public class User : TableEntity
    {
        public string AccessCode { get; set; }
        public string KindleEmail { get; set; }
        public DateTime LastProcessingDate { get; set; }
        public string PocketUsername { get; set; }
        public string Token { get; set; }

        public User()
        {
        }
    }
}