using ChatService.Domain.Entities;
using MongoDB.Bson;

namespace ChatService.Data.Interrfaces
{
    public interface IChatRepo
    {
        Task<Chat> GetChatByIdAsync(ObjectId chatId, CancellationToken cancellation);
        Task<bool> AddChatAsync(Chat chat);
        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId, CancellationToken cancellation);
        Task<IEnumerable<Chat>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation);

        Task<string> GetUserIdFromChat(string chatId,CancellationToken cancellation);
    }
}
