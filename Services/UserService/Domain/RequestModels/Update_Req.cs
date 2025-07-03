using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.RequestModels
{
    public class Update_Req
    {
        public string? UserId { get; set; }

        [StringLength(256), MinLength(3), RegularExpression(@"^[a-zA-Z0-9._-]+$")]
        public string? UserName { get; set; }

        [EmailAddress, StringLength(256), MinLength(5)]
        public string? Email { get; set; }

        [StringLength(15), MinLength(10), RegularExpression(@"^\+?[0-9\s-]+$")]
        public string? PhoneNumber { get; set; }
      

    }
}
