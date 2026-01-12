using LBRS.Book.DBContext.Models;
using Microsoft.EntityFrameworkCore;

namespace LBRS.Book.DBContext
{
    public class BookServiceDbContext : DbContext
    {
        public BookServiceDbContext(DbContextOptions<BookServiceDbContext> options) : base(options)
        {
        }
        public DbSet<BookDetail> BookDetails { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationStatus> ReservationStatuses { get; set; }
    }
}
