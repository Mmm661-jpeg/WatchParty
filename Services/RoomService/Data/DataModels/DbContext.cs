using MongoDB.Driver;
using RoomService.Domain.Entities;

namespace RoomService.Data.DataModels
{
    public class DbContext : IDbContext
    {
        private readonly IMongoDatabase _database;

        public DbContext(string connectionString, string databaseName)
        {
            var client =  new MongoClient(connectionString);
            _database =  client.GetDatabase(databaseName);
        }

        public IMongoCollection<Rooms> Rooms => _database.GetCollection<Rooms>("Rooms");

    }
}
