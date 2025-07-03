using System.ComponentModel.DataAnnotations;

namespace SyncService.Domain.Requests.RoomRequests
{
    public class UpdatePlaybackStateInRoomRequest
    {
        [StringLength(100,ErrorMessage = "RoomId cannot exceed 100 characters.")]
        public string RoomId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Position must be a non-negative number.")]
      

        public double Position { get; set; }

        
        public bool IsPaused { get; set; }

        [StringLength(100, ErrorMessage = "VideoId cannot exceed 100 characters.")]

        public string VideoId { get; set; }
    }
}
