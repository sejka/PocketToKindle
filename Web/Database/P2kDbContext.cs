using Core;
using Microsoft.EntityFrameworkCore;

namespace Web.Database
{
    public class P2kDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ReportedArticle> ReportedArticles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("DataSource=db/p2k.db");
        }
    }
}