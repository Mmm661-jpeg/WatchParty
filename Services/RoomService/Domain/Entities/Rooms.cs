using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RoomService.Domain.Entities
{
    public class Rooms
    {
        [BsonId] // Marks this as the primary key (_id)
        [BsonRepresentation(BsonType.ObjectId)] // Allows it to be stored as ObjectId but used as string in C#
        public string Id { get; set; }
        public string RoomId { get; set; } = Guid.NewGuid().ToString("N").Substring(0,7);
        public string RoomName { get; set; } = string.Empty;
        public int MaxOccupancy { get; set; } = 20;

        public string HostId { get; set; } = null!;

        public List<string> ParticipantIds { get; set; } = new();

        public string? CurrentVideoId { get; set; }


        public double CurrentPlaybackPosition { get; set; } = 0;


        public bool IsPaused { get; set; } = true;

        public DateTime LastSyncUpdate { get; set; } = DateTime.UtcNow;

        public bool IsPrivate { get; set; } = false;
        public string? AccessCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Rooms() { }

       
    }
}
