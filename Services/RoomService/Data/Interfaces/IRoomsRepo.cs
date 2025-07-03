using RoomService.Domain.Entities;

namespace RoomService.Data.Interfaces
{
    public interface IRoomsRepo
    {
        Task<Rooms> GetRoomByIdsync(string roomId,CancellationToken cancellation);

        Task<List<Rooms>> GetAllRoomsAsync(CancellationToken cancellation);

        Task<List<Rooms>> GetPublicRoomsAsync(int page,CancellationToken cancellation, int pagesize);

        Task<List<Rooms>> GetPrivateRoomsAsync(CancellationToken cancellation);

        Task<Rooms> CreateRoomAsync(Rooms room,CancellationToken cancellation);

        Task<Rooms?> UpdateRoomAsync(string roomId,string hostId,string? roomName,int? maxOccupancy,CancellationToken cancellation);

        Task<bool> DeleteRoomAsync(string roomId, CancellationToken cancellation);

        Task<string> GenerateNewRoomPassAsync(string roomId, string newPass,CancellationToken cancellation);

        Task<bool> JoinRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<bool> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<bool> RoomExistsAsync(string roomId,CancellationToken cancellation);

        Task<List<Rooms>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation,int page,int pageSize);

        Task<List<Rooms>> GetRoomsByHostAsync(string hostId, CancellationToken cancellation);

        Task<bool> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused, string videoId,CancellationToken cancellation);

        Task<bool> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation);

        Task<bool> IsUserInRoomAsync(string roomId, string userId,CancellationToken cancellation);

        Task<bool> IsRoomFullAsync(string roomId, CancellationToken cancellation);

        Task<string> MakeRoomPrivateAsync(string roomId,string newPass,CancellationToken cancellation);

        Task<bool> MakeRoomPublicAsync(string roomId, CancellationToken cancellation);

        Task<string> GetAccessCode(string roomId, string hostId,CancellationToken cancellation);

        Task<bool> UpdateCurrentVideoIdAsync(string roomId, string videoId, CancellationToken cancellation);
    }
}
