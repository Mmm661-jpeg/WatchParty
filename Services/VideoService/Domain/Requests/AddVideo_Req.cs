using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using VideoService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace VideoService.Domain.Requests
{
    public class AddVideo_Req
    {
        [Required, MaxLength(20)]
        public string VideoId { get; set; }

        [Required]
        public Platform Platform { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        [Url(ErrorMessage = "Invalid URL format for ThumbnailUrl.")]
        public string ThumbnailUrl { get; set; }

        [Range(1, 36000)]
        public double Duration { get; set; }

        [Required, MaxLength(100)]
        public string AddedByUserId { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required, MaxLength(200)]
        public string ChannelName { get; set; }

        [Required, MaxLength(20)]
        public string RoomId { get; set; }


    }
}
