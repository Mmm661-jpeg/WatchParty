using MongoDB.Driver;
using VideoService.Domain.Entities;

namespace VideoService.Data.DataModels
{
    public class DbContext : IDbContext
    {
        private readonly IMongoDatabase _database;
        public DbContext(IMongoClient client, string databaseName)
        {
            _database = client.GetDatabase(databaseName);
        }
        public IMongoCollection<Video> Videos => _database.GetCollection<Video>("Videos");
    }
}
