using Microsoft.EntityFrameworkCore;
using Snapfish.BL.Models;

namespace Snapfish.API.Database
{
    public class SnapContext : DbContext
    {
        public SnapContext(DbContextOptions<SnapContext> options)
            : base(options)
        {
        }
        public DbSet<SnapUser> SnapUsers { get; set; }
        public DbSet<SnapMessage> SnapMessages { get; set; }
        public DbSet<SnapMetadata> SnapMetadatas { get; set; }
        public DbSet<SnapReceiver> SnapReceivers { get; set; }
        public DbSet<Snap> Snaps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SnapReceiver>()
                .HasKey(r => new { r.SnapMessageID, r.SnapUserID });
        }
        
    }
}
