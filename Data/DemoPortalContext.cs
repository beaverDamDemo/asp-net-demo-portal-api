using AspNetDemoPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetDemoPortalAPI.Data
{
    public class DemoPortalContext : DbContext
    {
        public DemoPortalContext(DbContextOptions<DemoPortalContext> options) : base(options)
        {
        }
        public DbSet<AspNetDemoPortalAPI.Models.User> Users { get; set; }
        public DbSet<Todo> Todos { get; set; }
    }
}
