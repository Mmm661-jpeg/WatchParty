using MongoDB.Driver;
using VideoService.Domain.Entities;

namespace VideoService.Data.DataModels
{
    public interface IDbContext
    {
        IMongoCollection<Video> Videos { get; }
    }
}
