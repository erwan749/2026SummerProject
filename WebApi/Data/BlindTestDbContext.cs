using Entities;
using Microsoft.EntityFrameworkCore;
namespace WebApi.Data
{
    public class BlindTestDbContext : DbContext
    {
        public BlindTestDbContext(DbContextOptions<BlindTestDbContext> options) : base(options)
        { 

        }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<BlindTest> BlindTests { get; set; }
         
    }
}
