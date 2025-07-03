using System.ComponentModel.DataAnnotations;

namespace WatchPartyClient.Models
{
    public class CreateRoom_Req
    {

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Room name must be between 3 and 100 characters.")]
        public string RoomName { get; set; }

        [Range(1, 20, ErrorMessage = "Max occupancy must be between 1 and 20.")]
        public int MaxOccupancy { get; set; } = 20;

        [Required(ErrorMessage = "Host ID is required.")]

        public string HostId { get; set; } = null!;

        public bool IsPrivate { get; set; } = false;
    }
}
