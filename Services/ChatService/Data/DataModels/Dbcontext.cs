using ChatService.Domain.Entities;
using MongoDB.Driver;

namespace ChatService.Data.DataModels
{
    public class DbContext:IDbContext
    {
        private readonly IMongoDatabase _database;

        public DbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Chat> Chats => _database.GetCollection<Chat>("Chats");
    }
}
