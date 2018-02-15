using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Core
{
    public class User : TableEntity
    {
        public string AccessCode { get; set; }
        public string KindleEmail { get; set; }
        public DateTime LastProcessingDate { get; set; }
        public string PocketUsername { get; set; }

        public User()
        {
        }
    }
}