using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.RequestModels
{
    public class AnonUserRequest
    {
        [StringLength(100, ErrorMessage = "UserId cannot be longer than 100 characters.")]
        public string CurrentRoomId { get; set; }

        [StringLength(100, ErrorMessage = "UserId cannot be longer than 100 characters.")]
        public string  UserName { get; set; }
    }
}
