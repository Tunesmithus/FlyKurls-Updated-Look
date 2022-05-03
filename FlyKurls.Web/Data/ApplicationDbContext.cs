using FlyKurls.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FlyKurls.Web.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Category> categories{ get; set; }
    }
}
