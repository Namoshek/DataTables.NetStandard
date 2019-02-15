using DataTables.NetCore.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetCore.Sample
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<Person> Persons { get; set; }

        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }
    }
}
