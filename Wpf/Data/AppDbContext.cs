using Microsoft.EntityFrameworkCore;
using System.IO;
using Wpf.Models;

namespace Wpf.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.Combine(System.AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "todo.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
