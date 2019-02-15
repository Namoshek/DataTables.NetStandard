using DataTables.NetStandard.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetStandard.Sample
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<Person> Persons { get; set; }

        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }
    }
}
