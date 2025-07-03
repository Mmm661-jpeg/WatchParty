using System.ComponentModel.DataAnnotations;

namespace SyncService.Domain.Requests.RoomRequests
{
    public class UpdateVideoIdRequest
    {
        [StringLength(100, ErrorMessage = "Video ID cannot exceed 100 characters.")]
        public string VideoId { get; set; }
    }
}
