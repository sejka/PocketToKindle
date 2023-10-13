using System;

namespace Core
{
    public interface IArticle
    {
        string Content { get; set; }
        DateTime? DatePublished { get; set; }
        string Title { get; set; }
    }
}