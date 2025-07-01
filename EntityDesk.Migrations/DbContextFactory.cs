using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EntityDesk.Migrations
{
    public class EntityDeskDbContextFactory : IDesignTimeDbContextFactory<EntityDeskDbContext>
    {
        public EntityDeskDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<EntityDeskDbContext>();
            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 0))
            );

           
            return new EntityDeskDbContext(optionsBuilder.Options);
        }
    }
} 