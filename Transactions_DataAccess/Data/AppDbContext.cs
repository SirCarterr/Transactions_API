using Microsoft.EntityFrameworkCore;

namespace Transactions_DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
