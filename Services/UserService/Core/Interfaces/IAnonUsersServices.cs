using UserService.Domain.UtilModels;

namespace UserService.Core.Interfaces
{
    public interface IAnonUsersServices
    {
        Task<OperationResult<string>> GenerateAnonUserToken(string currentRoomId, string userName);


    }
}
