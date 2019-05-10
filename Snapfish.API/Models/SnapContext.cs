using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Snapfish.API.Models
{
    public class SnapContext : DbContext
    {
        public SnapContext(DbContextOptions<SnapContext> options)
            : base(options)
        {
        }
        public DbSet<SnapMessage> SnapMessages { get; set; }
        public DbSet<EchogramInfo> EchogramInfos { get; set; }
    }
}
