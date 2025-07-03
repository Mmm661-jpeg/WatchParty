using UserService.Domain.Entities;

namespace UserService.Data.Interfaces
{
    public interface IUserRepos
    {
        Task<User?> GetUserByIdAsync(string userId , CancellationToken cancellationToken = default);

        Task<User?> GetUserByEmailAsync(string email , CancellationToken cancellationToken = default);

        Task<User?> GetUserByUsernameAsync(string username , CancellationToken cancellationToken = default);

        Task<IEnumerable<User>> GetAllUsersAsync(int page = 1, CancellationToken cancellationToken = default);


        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

        Task<bool> UpdateLastActiveAsync(string userId, CancellationToken cancellationToken = default);

        


    }
}
