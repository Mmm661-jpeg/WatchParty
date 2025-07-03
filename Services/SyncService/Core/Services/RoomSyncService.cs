using Grpc.Core;
using SharedProtos;
using SyncService.Core.Interfaces;
using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.RoomRequests;
using SyncService.Domain.UtilModels;
using System.Text.RegularExpressions;

namespace SyncService.Core.Services
{
    public class RoomSyncService : IRoomSyncService
    {
        private readonly ILogger<RoomSyncService> _logger;
        private readonly RoomService.RoomServiceClient _roomServiceClient;

        public RoomSyncService(ILogger<RoomSyncService> logger, RoomService.RoomServiceClient roomServiceClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roomServiceClient = roomServiceClient ?? throw new ArgumentNullException(nameof(roomServiceClient));
        }

        public async Task<OperationResult<RoomDto>> CreateRoomAsync(CreateRoom_Req req, CancellationToken cancellation)
        {
            if (req == null)
            {
                return OperationResult<RoomDto>.Failure(null, "Invalid request data");
            }

            if (string.IsNullOrWhiteSpace(req.RoomName) || string.IsNullOrWhiteSpace(req.HostId))
            {
                return OperationResult<RoomDto>.Failure(null, "Room name and host ID are required");
            }


            try
            {
                var grpcRequest = new CreateRoomRequest
                {
                    RoomName = req.RoomName,
                    MaxOccupancy = req.MaxOccupancy,
                    HostId = req.HostId,
                    IsPrivate = req.IsPrivate,

                };

                var result = await _roomServiceClient.CreateRoomAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<RoomDto>.Failure(null, "Failed to create room");
                }

                var room = result.Room;

                var roomDto = new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList(),


                };

                return OperationResult<RoomDto>.Success(roomDto, "Room created successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Room not created for room with name ID: {RoomName}. Message: {Message}", req.RoomName, errorMessage);

                return OperationResult<RoomDto>.Failure(null, "Room not created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return OperationResult<RoomDto>.Error(ex, "Error creating room");
            }
        }

        public async Task<OperationResult<bool>> DeleteRoomAsync(string roomId, string userId, bool isAdmin, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {

                var roomToDelete = await GetRoomByIdsync(roomId, cancellation);

                if (roomToDelete == null)
                {
                    return OperationResult<bool>.Failure(false, "Room not found");
                }
                if (!roomToDelete.IsSuccess || roomToDelete.Data == null)
                {
                    return OperationResult<bool>.Failure(false, roomToDelete.Message);
                }

                if (roomToDelete.Data.HostId != userId && !isAdmin)
                {
                    return OperationResult<bool>.Failure(false, "You do not have permission to delete this room");
                }

                var grpcRequest = new DeleteRoomRequest
                {
                    RoomId = roomId
                };
                var result = await _roomServiceClient.DeleteRoomAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to delete room");
                }
                return OperationResult<bool>.Success(true, "Room deleted successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to delete room with ID: {RoomId}. Message: {Message}", roomId, errorMessage);
                return OperationResult<bool>.Failure(false, "Failed to delete room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error deleting room");

            }
        }

        public async Task<OperationResult<string>> GenerateNewRoomPassAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<string>.Failure(null, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<string>.Failure(null, "Invalid user ID");
            }
            try
            {
                var roomToUpdate = await GetRoomByIdsync(roomId, cancellation);
                if (roomToUpdate == null)
                {
                    return OperationResult<string>.Failure(null, "Room not found");
                }
                if (!roomToUpdate.IsSuccess || roomToUpdate.Data == null)
                {
                    return OperationResult<string>.Failure(null, roomToUpdate.Message);
                }
                if (roomToUpdate.Data.HostId != userId)
                {
                    return OperationResult<string>.Failure(null, "You do not have permission to generate a new room pass for this room");
                }

                var grpcRequest = new GenerateNewRoomPassRequest
                {
                    RoomId = roomId
                };
                var result = await _roomServiceClient.GenerateNewRoomPassAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<string>.Failure(null, "Failed to generate new room pass");
                }

                return OperationResult<string>.Success(result.NewAccessCode, "New room pass generated successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to generate new room pass for room with ID: {RoomId}. Message: {Message}", roomId, errorMessage);
                return OperationResult<string>.Failure(null, "Failed to generate new room pass for room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating new room pass for room ID: {RoomId}", roomId);

                return OperationResult<string>.Error(ex, "Error generating new room pass");
            }
        }

        public async Task<OperationResult<string>> GetAccessCode(string roomId, string hostId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<string>.Failure(null, "Invalid room ID");
            }

            if (string.IsNullOrWhiteSpace(hostId))
            {
                return OperationResult<string>.Failure(null, "Invalid host ID");
            }

            try
            {
                var grpcRequest = new GetAccessCodeRequest
                {
                    RoomId = roomId,
                    HostId = hostId
                };

                var result = await _roomServiceClient.GetAccessCodeAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<string>.Failure(null, "Failed to get access code");
                }

                return OperationResult<string>.Success(result.AccessCode, "Access code retrieved successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to get access code for room with ID: {RoomId}. Message: {Message}", roomId, errorMessage);

                return OperationResult<string>.Failure(null, "Failed to get access code for room ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access code for room ID: {RoomId}", roomId);
                return OperationResult<string>.Error(ex, "Error getting access code");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetPublicRoomsAsync(int page, CancellationToken cancellation, int pageSize = 100)
        {
            if (page < 1 || pageSize < 1)
            {
                return OperationResult<List<RoomDto>>.Failure(null, "Invalid pagination parameters");
            }
            try
            {
                var grpcRequest = new GetPublicRoomsRequest
                {
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _roomServiceClient.GetPublicRoomsAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null || result.Rooms.Count == 0)
                {
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No public rooms found");
                }

                var roomDtos = result.Rooms.Select(room => new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList()
                }).ToList();

                return OperationResult<List<RoomDto>>.Success(roomDtos, "Public rooms fetched successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to fetch public rooms: {Message}", errorMessage);

                return OperationResult<List<RoomDto>>.Failure(null, "Failed to fetch public rooms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public rooms");
                return OperationResult<List<RoomDto>>.Error(ex, "Error fetching public rooms");
            }
        }

        public async Task<OperationResult<RoomDto>> GetRoomByIdsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<RoomDto>.Failure(null, "Invalid room ID");
            }
            try
            {
                var grpcRequest = new GetRoomByIdRequest
                {
                    RoomId = roomId
                };
                var result = await _roomServiceClient.GetRoomByIdAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<RoomDto>.Failure(null, "Room not found");
                }
                var room = result.Room;

                var roomDto = new RoomDto
                {
                    RoomId = room.RoomId ?? string.Empty,
                    RoomName = room.RoomName ?? string.Empty,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId ?? string.Empty,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId ?? string.Empty,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList()
                };

                return OperationResult<RoomDto>.Success(roomDto, "Room fetched successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to fetch room with ID: {RoomId}. Message: {Message}", roomId, errorMessage);

                return OperationResult<RoomDto>.Failure(null, "Failed to fetch room with ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching room by ID: {RoomId}", roomId);
                return OperationResult<RoomDto>.Error(ex, "Error fetching room by ID");

            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetRoomsByHostAsync(string hostId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                return OperationResult<List<RoomDto>>.Failure(null, "Invalid host ID");
            }
            try
            {
                var grpcRequest = new GetRoomsByHostRequest
                {
                    HostId = hostId
                };
                var result = await _roomServiceClient.GetRoomsByHostAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null || result.Rooms.Count == 0)
                {
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No rooms found for the specified host");
                }

                var roomDtos = result.Rooms.Select(room => new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList()
                }).ToList();

                return OperationResult<List<RoomDto>>.Success(roomDtos, "Rooms fetched successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to fetch rooms by host ID: {HostId}. Message: {Message}", hostId, errorMessage);

                return OperationResult<List<RoomDto>>.Failure(null, "Failed to fetch rooms by host ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rooms by host ID: {HostId}", hostId);
                return OperationResult<List<RoomDto>>.Error(ex, "Error fetching rooms by host ID");
            }
        }

        public async Task<OperationResult<bool>> IsRoomFullAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            try
            {
                var grpcRequest = new IsRoomFullRequest
                {
                    RoomId = roomId
                };
                var result = await _roomServiceClient.IsRoomFullAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to check if room is full");
                }
                return OperationResult<bool>.Success(result.IsFull, "Room full status checked successfully");
            }
            catch (RpcException rpcEx)
            {
                _logger.LogWarning("Failed to check if room is full for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Failure(false, rpcEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room is full for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error checking if room is full");
            }
        }

        public async Task<OperationResult<bool>> IsUserInRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {
                var grpcRequest = new IsUserInRoomRequest
                {
                    RoomId = roomId,
                    UserId = userId
                };

                var result = await _roomServiceClient.IsUserInRoomAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to check if user is in room");
                }

                return OperationResult<bool>.Success(result.IsInRoom, "User in room status checked successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to check if user is in room with ID: {RoomId} for user ID: {UserId}. Message: {Message}", roomId, userId, errorMessage);

                return OperationResult<bool>.Failure(false, "Failed to check if user is in room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is in room with ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error checking if user is in room");
            }
        }

        public async Task<OperationResult<bool>> JoinRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {
                var grpcRequest = new JoinRoomRequest
                {
                    RoomId = roomId,
                    UserId = userId
                };

                var result = await _roomServiceClient.JoinRoomAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    

                    _logger.LogWarning("JoinRoomAsync: gRPC returned null");
                    return OperationResult<bool>.Failure(false, "gRPC returned null, failed to join room.");
                }

                if (!result.Success)
                {
                    _logger.LogWarning("JoinRoomAsync: gRPC join failed");
                    return OperationResult<bool>.Failure(false, "Join failed");
                }
                return OperationResult<bool>.Success(result.Success, "Room joined successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to join room with ID: {RoomId} for user ID: {UserId}. Message: {Message}", roomId, userId, errorMessage);

                
                return OperationResult<bool>.Failure(false, $"Failed too join room, gRPC error: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room with ID: {RoomId} for user ID: {UserId}", roomId, userId);
                return OperationResult<bool>.Error(ex, "Error joining room");
            }
        }

        public async Task<OperationResult<bool>> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {
                var grpcRequest = new LeaveRoomRequest
                {
                    RoomId = roomId,
                    UserId = userId
                };

                var result = await _roomServiceClient.LeaveRoomAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to leave room");
                }

                return OperationResult<bool>.Success(result.Success, "Room left successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to leave room with ID: {RoomId} for user ID: {UserId}. Message: {Message}", roomId, userId, errorMessage);

                return OperationResult<bool>.Failure(false, "Failed to leave room with ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room with ID: {RoomId} for user ID: {UserId}", roomId, userId);
                return OperationResult<bool>.Error(ex, "Error leaving room");
            }
        }

        public async Task<OperationResult<string>> MakeRoomPrivateAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<string>.Failure(null, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<string>.Failure(null, "Invalid user ID");
            }
            try
            {
                var roomToUpdate = await GetRoomByIdsync(roomId, cancellation);
                if (roomToUpdate == null)
                {
                    return OperationResult<string>.Failure(null, "Room not found");
                }
                if (!roomToUpdate.IsSuccess || roomToUpdate.Data == null)
                {
                    return OperationResult<string>.Failure(null, roomToUpdate.Message);
                }
                if (roomToUpdate.Data.HostId != userId)
                {
                    return OperationResult<string>.Failure(null, "You do not have permission to make this room private");
                }
                var grpcRequest = new MakeRoomPrivateRequest
                {
                    RoomId = roomId
                };

                var result = await _roomServiceClient.MakeRoomPrivateAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<string>.Failure(null, "Failed to make room private");
                }

                if (!result.Success)
                {
                    return OperationResult<string>.Failure(null, "Room is already private or an error occurred");
                }

                return OperationResult<string>.Success(result.NewAccessCode, "Room made private successfully");



            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to make room private for room ID: {RoomId}. Message: {Message}", roomId, errorMessage);

                return OperationResult<string>.Failure(null, "Failed to make room privat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room private for room ID: {RoomId}", roomId);
                return OperationResult<string>.Error(ex, "Error making room private");
            }
        }

        public async Task<OperationResult<bool>> MakeRoomPublicAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {
                var roomToUpdate = await GetRoomByIdsync(roomId, cancellation);
                if (roomToUpdate == null)
                {
                    return OperationResult<bool>.Failure(false, "Room not found");
                }
                if (!roomToUpdate.IsSuccess || roomToUpdate.Data == null)
                {
                    return OperationResult<bool>.Failure(false, roomToUpdate.Message);
                }
                if (roomToUpdate.Data.HostId != userId)
                {
                    return OperationResult<bool>.Failure(false, "You do not have permission to make this room public");
                }

                var grpcRequest = new MakeRoomPublicRequest { RoomId = roomId };

                var result = await _roomServiceClient.MakeRoomPublicAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to make room public");
                }

                if (!result.Success)
                {
                    return OperationResult<bool>.Failure(false, "Room is already public or an error occurred");
                }

                return OperationResult<bool>.Success(true, "Room made public successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to make room public for room ID: {RoomId}. Message: {Message}", roomId, errorMessage);

                return OperationResult<bool>.Failure(false, "Failed to make room public");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room public for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error making room public");
            }
        }

        public async Task<OperationResult<bool>> RoomExistsAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            try
            {
                var grpcRequest = new RoomExistsRequest
                {
                    RoomId = roomId
                };

                var result = await _roomServiceClient.RoomExistsAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to check if room exists");
                }

                return OperationResult<bool>.Success(result.Exists, "Room existence checked successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogError("Failed to check if room exists for room ID: {RoomId}. Message: {Message}", roomId, errorMessage);

                return OperationResult<bool>.Failure(false, "Failed to check if room exists");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, " Error checking if room exists for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error checking if room exists");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return OperationResult<List<RoomDto>>.Failure(null, "Invalid search parameters");
            }
            if (page < 1 || pageSize < 1)
            {
                return OperationResult<List<RoomDto>>.Failure(null, "Invalid pagination parameters");
            }
            try
            {
                var grpcRequest = new SearchRoomRequest
                {
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _roomServiceClient.SearchRoomAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null || result.Rooms.Count == 0)
                {
                    return OperationResult<List<RoomDto>>.Failure(new List<RoomDto>(), "No rooms found matching the search term");
                }

                var grpcRooms = result.Rooms;

                var roomDtos = grpcRooms.Select(room => new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList()
                }).ToList();

                return OperationResult<List<RoomDto>>.Success(roomDtos, "Rooms searched successfully");



            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed searching rooms with term: {SearchTerm}. Error: {Message}", searchTerm, errorMessage);

                return OperationResult<List<RoomDto>>.Failure(null, "Failed searching rooms with term");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching rooms with term: {SearchTerm}", searchTerm);
                return OperationResult<List<RoomDto>>.Error(ex, "Error searching rooms");
            }
        }

        public async Task<OperationResult<bool>> UpdateCurrentVideoIdAsync(string roomId, string videoId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
           
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<bool>.Failure(false, "Invalid user ID");
            }
            try
            {
                var roomToUpdate = await GetRoomByIdsync(roomId, cancellation);
                if (roomToUpdate == null)
                {
                    return OperationResult<bool>.Failure(false, "Room not found");
                }
                if (!roomToUpdate.IsSuccess || roomToUpdate.Data == null)
                {
                    return OperationResult<bool>.Failure(false, roomToUpdate.Message);
                }
                if (roomToUpdate.Data.HostId != userId)
                {
                    return OperationResult<bool>.Failure(false, "You do not have permission to update the current video ID for this room");
                }
                var grpcRequest = new UpdateCurrentVideoIdRequest
                {
                    RoomId = roomId,
                    NewVideoId = videoId
                };
                var result = await _roomServiceClient.UpdateCurrentVideoIdAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to update current video ID");
                }

                return OperationResult<bool>.Success(result.Success, "Current video ID updated successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);

                _logger.LogWarning("Failed to update current video ID for room ID: {RoomId}. Error: {Message}", roomId, errorMessage);
                return OperationResult<bool>.Failure(false, "Failed to update current video");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current video ID for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error updating current video ID");
            }
        }

        public async Task<OperationResult<bool>> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused, string videoId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return OperationResult<bool>.Failure(false, "Invalid video ID");
            }
            if (position < 0)
            {
                return OperationResult<bool>.Failure(false, "Invalid playback position");
            }

            try
            {
                var grpcRequest = new UpdatePlaybackStateRequest
                {
                    RoomId = roomId,
                    Position = position,
                    IsPaused = isPaused,
                    VideoId = videoId
                };
                var result = await _roomServiceClient.UpdatePlaybackStateAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to update playback state");
                }
                return OperationResult<bool>.Success(result.Success, "Playback state updated successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to update playback state for room ID: {RoomId}. Error: {Message}", roomId, errorMessage);

                return OperationResult<bool>.Failure(false, "Failed to update playback state for room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playback state for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error updating playback state");
            }
        }

        public async Task<OperationResult<PlaybackStateDto>> GetPlaybackStateForRoom(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<PlaybackStateDto>.Failure(null, "Invalid room ID");
            }

            try
            {
                var grpcRequest = new GetRoomByIdRequest
                {
                    RoomId = roomId
                };

                var result = await _roomServiceClient.GetPlaybackStateAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null)
                {
                    return OperationResult<PlaybackStateDto>.Failure(null, "Failed to get playback state for room");
                }
                var playbackState = new PlaybackStateDto
                {
                    RoomId = result.RoomId,
                    VideoId = result.CurrentVideoId,
                    Position = result.CurrentPlaybackPosition,
                    IsPaused = result.IsPaused,
                    LastSyncUpdate = result.LastSyncUpdate.ToDateTime()
                };

                return OperationResult<PlaybackStateDto>.Success(playbackState, "Playback state retrieved successfully");
            }
            catch (RpcException ex)
            {
                var errorMessage = TranslateRpcErrorMessage(ex.Message);
                _logger.LogWarning("Failed to get playback state for room ID: {RoomId}. Error: {Message}", roomId, errorMessage);

                return OperationResult<PlaybackStateDto>.Failure(null, "Failed to get playback state for room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playback state for room ID: {RoomId}", roomId);
                return OperationResult<PlaybackStateDto>.Error(ex, "Error getting playback state");
            }
        }








        public async Task<OperationResult<RoomDto>> UpdateRoomAsync(UpdateRoom_Req req, CancellationToken cancellation)
        {
            if (req == null)
            {
                return OperationResult<RoomDto>.Failure(null, "Invalid request data");
            }
            try
            {
                var grpcRequest = new UpdateRoomRequest
                {
                    RoomId = req.RoomId,
                    RoomName = req.RoomName,
                    MaxOccupancy = req.MaxOccupancy,
                    HostId = req.HostId,

                };

                var result = await _roomServiceClient.UpdateRoomAsync(grpcRequest, cancellationToken: cancellation);
                if (result == null)
                {
                    return OperationResult<RoomDto>.Failure(null, "Failed to update room");
                }

                var room = result.Room;

                var roomDto = new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    IsPrivate = room.IsPrivate,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate.ToDateTime(),
                    CreatedAt = room.CreatedAt.ToDateTime(),
                    ParticipantIds = room.ParticipantIds.ToList()
                };

                return OperationResult<RoomDto>.Success(roomDto, "Room updated successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to update room with ID: {RoomId}. Error: {Message}", req.RoomId, errorMessage);
                return OperationResult<RoomDto>.Failure(null, "Failed to update room with ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room");
                return OperationResult<RoomDto>.Error(ex, "Error updating room");
            }
        }

        public async Task<OperationResult<bool>> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<bool>.Failure(false, "Invalid room ID");
            }
            if (string.IsNullOrWhiteSpace(accessCode))
            {
                return OperationResult<bool>.Failure(false, "Invalid access code must have value");
            }

            try
            {
                var grpcRequest = new ValidateRoomPassRequest
                {
                    RoomId = roomId,
                    AccessCode = accessCode
                };

                var result = await _roomServiceClient.ValidateRoomPassAsync(grpcRequest, cancellationToken: cancellation);

                if (result == null)
                {
                    return OperationResult<bool>.Failure(false, "Failed to validate room pass");
                }
                return OperationResult<bool>.Success(result.IsValid, "Room pass validated successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to validate room pass for room ID: {RoomId} with access code: {AccessCode}. Error: {Message}", roomId, accessCode, errorMessage);
                return OperationResult<bool>.Failure(false, "Failed to validate room pass for room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating room pass for room ID: {RoomId} with access code: {AccessCode}", roomId, accessCode);
                return OperationResult<bool>.Error(ex, "Error validating room pass");
            }
        }

        private static string TranslateRpcErrorMessage(string errorMessage)
        {
            var replacedString = errorMessage.Replace("RpcException: ", string.Empty)
                                     .Replace("Status(", string.Empty)
                                     .Replace(")", string.Empty)
                                     .Replace("Internal", "An internal server error occurred")
                                     .Replace("NotFound", "The requested resource was not found")
                                     .Replace("InvalidArgument", "Invalid argument provided")
                                     .Replace("AlreadyExists", "Resource already exists")
                                     .Replace("PermissionDenied", "Permission denied")
                                     .Replace("Unauthenticated", "User is not authenticated");



            replacedString = Regex.Replace(replacedString, @"\s+", " ");

            return replacedString.Trim();



        }
    }
}
