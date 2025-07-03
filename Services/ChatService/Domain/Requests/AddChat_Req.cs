using System.ComponentModel.DataAnnotations;

namespace ChatService.Domain.Requests
{
    public class AddChat_Req
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "RoomId is required and cannot be empty.")]
        [StringLength(100, ErrorMessage = "RoomId cannot exceed 100 characters.")]
        public string RoomId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "UserId is required and cannot be empty.")]
        [StringLength(100, ErrorMessage = "UserId cannot exceed 100 characters.")]
        public string UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required and cannot be empty.")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Message is required and cannot be empty.")]
        [StringLength(100, ErrorMessage = "Message cannot exceed 100 characters.")]
        public string Message { get; set; }

     


    }
}
