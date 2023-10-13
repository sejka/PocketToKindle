using System.ComponentModel.DataAnnotations;

namespace Web.Database
{
    public class ReportedArticle
    {
        [Key]
        public string Url { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}