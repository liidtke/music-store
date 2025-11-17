using Microsoft.EntityFrameworkCore;

namespace Hs.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(new MariaDbServerVersion("11.5.2"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // services.AddDbContext<DataContext>(options =>
        //               options.UseMySql(Configuration.GetConnectionString("MariaDbConnection"), new MariaDbServerVersion("11.5.2"))
        //                   // .LogTo(Console.WriteLine, LogLevel.Information)
        //                   // .EnableSensitiveDataLogging()
        //                   .EnableDetailedErrors()
        //           );
    }
}