using Microsoft.EntityFrameworkCore;
using System.Reflection;
using toDo.Models;

namespace toDo.WebDbContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<TodoTasks> tasks { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
