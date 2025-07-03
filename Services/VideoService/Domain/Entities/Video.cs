using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace VideoService.Domain.Entities
{
    public class Video
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("videoId")]
        public string VideoId { get; set; }

        [BsonElement("platform")]
        public Platform Platform { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [BsonElement("duration")]
        public double Duration { get; set; }

        [BsonElement("addedByUserId")]
        public string AddedByUserId { get; set; }

        [BsonElement("addedAt")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("channelName")]
        public string ChannelName { get; set; }

        [BsonElement("roomId")]
        public string RoomId { get; set; }

        [BsonElement("fullUrl")]

        public string FullUrl =>
            Platform switch
            {
                Platform.YouTube => $"https://www.youtube.com/embed/{VideoId}",
                //Platform.Vimeo => $"https://player.vimeo.com/video/{VideoId}",
                //Platform.Dailymotion => $"https://www.dailymotion.com/embed/video/{VideoId}",
                //Platform.Twitch => $"https://player.twitch.tv/?video={VideoId}&parent=yourdomain.com",
                _ => throw new NotSupportedException("Unsupported video platform")
            };
    }

    public enum Platform
    {
        YouTube,
        //Vimeo,
        //Dailymotion,
        //Twitch,
        //Other
    }
}
