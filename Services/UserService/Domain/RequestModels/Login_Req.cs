using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.RequestModels
{
    public class Login_Req
    {
        [Required]
        [StringLength(256, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
