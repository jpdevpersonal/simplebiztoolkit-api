using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace simplebiztoolkit_api.Data;

public class SimpleBizDbContextFactory : IDesignTimeDbContextFactory<SimpleBizDbContext>
{
    public SimpleBizDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SIMPLEBIZ_CONNECTION")
            ?? "Server=JPLAPTOP;Database=simplebiztoolkit;Integrated Security=SSPI;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<SimpleBizDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SimpleBizDbContext(optionsBuilder.Options);
    }
}
