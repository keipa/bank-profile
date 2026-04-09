using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankProfiles.Web.Data;

public class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
{
    public BankDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BankProfiles;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

        return new BankDbContext(optionsBuilder.Options);
    }
}
