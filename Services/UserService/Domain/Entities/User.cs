using Microsoft.AspNetCore.Identity;

namespace UserService.Domain.Entities
{
    public class User : IdentityUser
    {
        
        public DateTime CreatedAt { get; set; }

        public DateTime LastActive { get; set; }

        public UserRole Role { get; set; } = UserRole.User;


    }

    public enum UserRole
    {
        User,
        Admin,
        Moderator
    }
}
