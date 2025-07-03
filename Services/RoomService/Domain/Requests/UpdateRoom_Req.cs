using System.ComponentModel.DataAnnotations;

namespace RoomService.Domain.Requests
{
    public class UpdateRoom_Req
    {
        [Required(ErrorMessage = "Room ID is required.")]
        public string? RoomId { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Room name must be between 3 and 100 characters.")]
        public string? RoomName { get; set; }

        [Range(1, 20, ErrorMessage = "Max occupancy must be between 1 and 20.")]
        public int MaxOccupancy { get; set; }

        [Required(ErrorMessage = "Host ID is required.")]
        public string HostId { get; set; } = null!;

      

      


      

      
    }
}
