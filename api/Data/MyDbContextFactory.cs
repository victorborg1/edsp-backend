using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.Data
{
    public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var password = Environment.GetEnvironmentVariable("EIGENDSP_DB_PASSWORD");

            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException(
                    "EIGENDSP_DB_PASSWORD environment variable is not set."
                );

            var connectionString =
                $"Host=localhost;Port=5432;Database=eigendsp;Username=eigendsp;Password={password}";

            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
