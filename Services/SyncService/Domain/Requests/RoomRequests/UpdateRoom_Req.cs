using System.ComponentModel.DataAnnotations;

namespace SyncService.Domain.Requests.RoomRequests
{
    public class UpdateRoom_Req
    {
        [StringLength(100, ErrorMessage = "RoomId cannot be longer than 100 characters.")]
        public string? RoomId { get; set; }

        [StringLength(100, ErrorMessage = "RoomName cannot be longer than 100 characters.")]
        public string? RoomName { get; set; }

        [Range(1, 20, ErrorMessage = "MaxOccupancy must be between 1 and 20.")]
        public int MaxOccupancy { get; set; }

        [Required, StringLength(100, ErrorMessage = "HostId cannot be longer than 100 characters.")]
        public string HostId { get; set; } = null!;


    }
}
