using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.ChatRequests;
using SyncService.Domain.UtilModels;
using System.ComponentModel.DataAnnotations;

namespace SyncService.Core.Interfaces
{
    public interface IChatSyncService
    {
        Task<OperationResult<ChatDto>> GetChatByIdAsync(string chatId, CancellationToken cancellation);


        Task<OperationResult<bool>> AddChatAsync(AddChat_Req req,CancellationToken cancellation);


        Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByUserIdAsync(string userId, CancellationToken cancellation);

        Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<string>> GetUserIdFromChat(string chatId, CancellationToken cancellation);







    }
}
