using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data.DataModel
{
    public class DbContextFactory: IDesignTimeDbContextFactory<UserServiceDBContext>
    {
        public UserServiceDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string is not configured in appsettings.json");
            }
            // Create options builder with the connection string
            var optionsBuilder = new DbContextOptionsBuilder<UserServiceDBContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new UserServiceDBContext(optionsBuilder.Options);
        }
    }
    
    
}
