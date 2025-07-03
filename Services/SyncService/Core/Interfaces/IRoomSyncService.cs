using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.RoomRequests;
using SyncService.Domain.UtilModels;

namespace SyncService.Core.Interfaces
{
    public interface IRoomSyncService
    {
        Task<OperationResult<RoomDto>> GetRoomByIdsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<List<RoomDto>>> GetPublicRoomsAsync(int page, CancellationToken cancellation, int pageSize = 100);

        Task<OperationResult<RoomDto>> CreateRoomAsync(CreateRoom_Req req, CancellationToken cancellation);


        Task<OperationResult<RoomDto>> UpdateRoomAsync(UpdateRoom_Req req, CancellationToken cancellation);

        Task<OperationResult<bool>> DeleteRoomAsync(string roomId, string userId,bool isAdmin,CancellationToken cancellation);

        Task<OperationResult<string>> GenerateNewRoomPassAsync(string roomId, string userId,CancellationToken cancellation);

        Task<OperationResult<bool>> JoinRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<OperationResult<bool>> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<OperationResult<bool>> RoomExistsAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<List<RoomDto>>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation, int page, int pageSize);

        Task<OperationResult<List<RoomDto>>> GetRoomsByHostAsync(string hostId, CancellationToken cancellation);

        Task<OperationResult<bool>> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused, string videoId, CancellationToken cancellation);

        Task<OperationResult<PlaybackStateDto>> GetPlaybackStateForRoom(string roomId, CancellationToken cancellation);

        Task<OperationResult<bool>> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation);

        Task<OperationResult<bool>> IsUserInRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<OperationResult<bool>> IsRoomFullAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<string>> MakeRoomPrivateAsync(string roomId, string userId,CancellationToken cancellation);

        Task<OperationResult<bool>> MakeRoomPublicAsync(string roomId, string userId ,CancellationToken cancellation);

        Task<OperationResult<string>> GetAccessCode(string roomId, string hostId, CancellationToken cancellation);

        Task<OperationResult<bool>> UpdateCurrentVideoIdAsync(string roomId, string videoId, string userId,CancellationToken cancellation);
    }
}
