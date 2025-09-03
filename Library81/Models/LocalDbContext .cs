using Microsoft.EntityFrameworkCore;

namespace Library81.Models
{
    // LocalDbContext.cs (SQLite)
    public class LocalDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<LocalStorage> LocalStorages { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=library_local.db");
        }
    }
}
