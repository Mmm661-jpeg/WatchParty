using MongoDB.Driver;
using RoomService.Domain.Entities;

namespace RoomService.Data.DataModels
{
    public interface IDbContext
    {
        IMongoCollection<Rooms> Rooms { get; }
    }
}
