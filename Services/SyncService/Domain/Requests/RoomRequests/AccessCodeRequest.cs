using System.ComponentModel.DataAnnotations;

namespace SyncService.Domain.Requests.RoomRequests
{
    public class AccessCodeRequest
    {
        [StringLength(100, ErrorMessage = "Access code cannot exceed 100 characters.")]
        [Required(ErrorMessage = "Access code is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Access code can only contain alphanumeric characters.")]
        public string AccessCode { get; set; }
    }
}
