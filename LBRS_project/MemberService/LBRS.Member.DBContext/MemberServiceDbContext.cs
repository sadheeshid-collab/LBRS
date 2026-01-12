using LBRS.Member.DBContext.Models;
using Microsoft.EntityFrameworkCore;

namespace LBRS.Member.DBContext
{
    public class MemberServiceDbContext : DbContext
    {
        public MemberServiceDbContext(DbContextOptions<MemberServiceDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

    }
}
