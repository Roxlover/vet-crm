using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VetCrm.Infrastructure.Data;

public class VetCrmDbContextFactory : IDesignTimeDbContextFactory<VetCrmDbContext>
{
    public VetCrmDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=vetcrm;Username=postgres;Password=CHANGE_ME";

        var optionsBuilder = new DbContextOptionsBuilder<VetCrmDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new VetCrmDbContext(optionsBuilder.Options);
    }
}