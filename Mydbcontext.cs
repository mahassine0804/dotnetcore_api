using dbcontext_wallet_test.Models;
using Microsoft.EntityFrameworkCore;

namespace dbcontext_wallet_test.data
{
    public class Mydbcontext: DbContext
    {
        public Mydbcontext() { }

        public Mydbcontext(DbContextOptions<Mydbcontext> options) : base(options) { }

        public DbSet<wallets> wallets { get; set; }
    }
}
