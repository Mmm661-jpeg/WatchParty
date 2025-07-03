using ChatService.Domain.DTO_s;
using ChatService.Domain.Entities;
using ChatService.Domain.Requests;
using ChatService.Domain.UtilModels;

namespace ChatService.Core.Interfaces
{
    public interface IChatServices
    { 
        Task<OperationResult<ChatDto>> GetChatByIdAsync(string chatId, CancellationToken cancellation);

      
        Task<OperationResult<bool>> AddChatAsync(AddChat_Req req);

      
        Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByUserIdAsync(string userId,CancellationToken cancellation);

        Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<string>> GetUserIdFromChat(string chatId, CancellationToken cancellation);
    }
}
