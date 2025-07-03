using ChatService.Domain.Entities;
using MongoDB.Driver;

namespace ChatService.Data.DataModels
{
    public interface IDbContext
    {
        IMongoCollection<Chat> Chats { get; }

    }
}
