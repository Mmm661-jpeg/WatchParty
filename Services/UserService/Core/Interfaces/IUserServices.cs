using UserService.Domain.DTO_s;
using UserService.Domain.Entities;
using UserService.Domain.RequestModels;
using UserService.Domain.UtilModels;

namespace UserService.Core.Interfaces
{
    public interface IUserServices
    {
        Task<OperationResult<UserDTO>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<OperationResult<UserDTO>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<OperationResult<UserDTO>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

        Task<OperationResult<IReadOnlyList<UserDTO>>> GetAllUsersAsync(int page = 1, CancellationToken cancellationToken = default);


        Task<OperationResult<bool>> UpdateLastActiveAsync(string userId, CancellationToken cancellationToken = default);


        Task<OperationResult<bool>> UpdateUserAsync(Update_Req req, CancellationToken cancellationToken = default);

        Task<OperationResult<bool>> RegisterUserAsync(Register_Req req, CancellationToken cancellationToken = default);

        Task<OperationResult<bool>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

        Task<OperationResult<string>> LoginAsync(Login_Req login_Req, CancellationToken cancellationToken = default);

        Task<OperationResult<bool>> ChangeUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken = default);


    }
}
