using System.ComponentModel.DataAnnotations;

namespace Web.Database
{
    public class User
    {
        public User()
        {
        }

        public string AccessCode { get; set; }

        [Required]
        public string KindleEmail { get; set; }

        public DateTime LastProcessingDate { get; set; } = DateTime.UtcNow;

        [Key]
        [Required]
        public string PocketUsername { get; set; }

        [Required]
        public string Token { get; set; }
    }
}