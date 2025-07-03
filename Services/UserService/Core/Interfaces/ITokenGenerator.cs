using UserService.Domain.Entities;

namespace UserService.Core.Interfaces
{
    public interface ITokenGenerator
    {
        Task<string> GenerateTokenForUser(User user);

        Task<string> GenerateTokenForAnonUser(AnonUsers anonUser);
    }
}
