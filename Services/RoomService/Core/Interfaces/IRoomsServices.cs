using RoomService.Domain.DTO_s;
using RoomService.Domain.Entities;
using RoomService.Domain.Requests;
using RoomService.Domain.UtilModels;

namespace RoomService.Core.Interfaces
{
    public interface IRoomsServices
    {
        Task<OperationResult<RoomDto>> GetRoomByIdsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<List<RoomDto>>> GetAllRoomsAsync(CancellationToken cancellation);

        Task<OperationResult<List<RoomDto>>> GetPublicRoomsAsync(int page,CancellationToken cancellation,int pageSize);

        Task<OperationResult<List<RoomDto>>> GetPrivateRoomsAsync(CancellationToken cancellation);

        Task<OperationResult<RoomDto>>  CreateRoomAsync(CreateRoom_Req req,CancellationToken cancellation);

        Task<OperationResult<RoomDto>>  UpdateRoomAsync(UpdateRoom_Req req,CancellationToken cancellation);

        Task<OperationResult<bool>> DeleteRoomAsync(string roomId,CancellationToken cancellation);

        Task<OperationResult<string>> GenerateNewRoomPassAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<bool>> JoinRoomAsync(string roomId, string userId,CancellationToken cancellation);

        Task<OperationResult<bool>> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation);

        Task<OperationResult<bool>> RoomExistsAsync(string roomId,CancellationToken cancellation);

        Task<OperationResult<List<RoomDto>>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation, int page, int pageSize);

        Task<OperationResult<List<RoomDto>>> GetRoomsByHostAsync(string hostId, CancellationToken cancellation);

        Task<OperationResult<bool>> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused,string videoId,CancellationToken cancellation);

        Task<OperationResult<PlaybackStateDto>> GetPlaybackStateForRoom(string roomId, CancellationToken cancellation);

        Task<OperationResult<bool>> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation);

        Task<OperationResult<bool>> IsUserInRoomAsync(string roomId, string userId,CancellationToken cancellation);

        Task<OperationResult<bool>> IsRoomFullAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<string>> MakeRoomPrivateAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<bool>> MakeRoomPublicAsync(string roomId, CancellationToken cancellation);

        Task<OperationResult<string>> GetAccessCode(string roomId, string hostId, CancellationToken cancellation);

        Task<OperationResult<bool>> UpdateCurrentVideoIdAsync(string roomId, string videoId, CancellationToken cancellation);
    }
}

