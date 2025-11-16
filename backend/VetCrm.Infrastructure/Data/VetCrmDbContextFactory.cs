using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VetCrm.Infrastructure.Data;

public class VetCrmDbContextFactory : IDesignTimeDbContextFactory<VetCrmDbContext>
{
    public VetCrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VetCrmDbContext>();

        var connectionString =
            "Host=localhost;Port=5432;Database=vetcrm;Username=postgres;Password=Aa1010";

        optionsBuilder.UseNpgsql(connectionString);

        return new VetCrmDbContext(optionsBuilder.Options);
    }
}
