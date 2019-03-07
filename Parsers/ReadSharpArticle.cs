using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parsers
{
    class ReadSharpArticle : IArticle
    {
        public string Content { get; set; }
        public DateTime? DatePublished { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
