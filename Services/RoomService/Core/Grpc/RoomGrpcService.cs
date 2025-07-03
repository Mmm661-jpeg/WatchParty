using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RoomService.Core.Interfaces;
using RoomService.Domain.DTO_s;
using RoomService.Domain.UtilModels;
using SharedProtos;


namespace RoomService.Core.Grpc
{
    public class RoomGrpcService : SharedProtos.RoomService.RoomServiceBase
    {
        private readonly ILogger<RoomGrpcService> _logger;
        private readonly IRoomsServices _roomService;

        public RoomGrpcService(IRoomsServices roomsServices, ILogger<RoomGrpcService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roomService = roomsServices ?? throw new ArgumentNullException(nameof(roomsServices));
        }

        public override async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                _logger.LogWarning("CreateRoomRequest is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }
            if (string.IsNullOrWhiteSpace(request.RoomName) || string.IsNullOrWhiteSpace(request.HostId))
            {
                _logger.LogWarning("Room name and Host ID cannot be null or empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Room name and Host ID cannot be null or empty"));
            }
            if (request.MaxOccupancy <= 0)
            {
                _logger.LogWarning("Max occupancy must be greater than 0");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Max occupancy must be greater than 0"));
            }
            try
            {
                var createRoomRequest = new Domain.Requests.CreateRoom_Req
                {
                    RoomName = request.RoomName,
                    MaxOccupancy = request.MaxOccupancy,
                    HostId = request.HostId,
                    IsPrivate = request.IsPrivate
                };

                var result = await _roomService.CreateRoomAsync(createRoomRequest, context.CancellationToken);
                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new InvalidOperationException("Room creation returned null data");

                    var lastSyncUpdate = Timestamp.FromDateTime(resultData.LastSyncUpdate.ToUniversalTime());
                    var createdAt = Timestamp.FromDateTime(resultData.CreatedAt.ToUniversalTime());
                    var room = new GrpcRoom()
                    {
                        RoomId = resultData.RoomId ?? string.Empty,
                        RoomName = resultData.RoomName ?? string.Empty,
                        MaxOccupancy = resultData.MaxOccupancy,
                        HostId = resultData.HostId ?? string.Empty,
                        ParticipantIds = { resultData.ParticipantIds ?? new List<string>() },
                        CurrentVideoId = resultData.CurrentVideoId ?? string.Empty,
                        CurrentPlaybackPosition = resultData.CurrentPlaybackPosition,
                        IsPaused = resultData.IsPaused,
                        LastSyncUpdate = lastSyncUpdate,
                        IsPrivate = resultData.IsPrivate,
                        CreatedAt = createdAt,
                    };

                    return new CreateRoomResponse
                    {
                        Room = room
                    };
                }
                else
                {

                    _logger.LogWarning("Failed to create room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }


            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<DeleteRoomResponse> DeleteRoom(DeleteRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("DeleteRoomRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.DeleteRoomAsync(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new DeleteRoomResponse
                    {
                        Success = true,

                    };
                }
                else
                {
                    _logger.LogWarning("Failed to delete room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }


            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetPublicRoomsResponse> GetPublicRooms(GetPublicRoomsRequest request, ServerCallContext context)
        {
            if (request == null || request.Page < 1 || request.PageSize < 1)
            {
                _logger.LogWarning("GetPublicRoomsRequest is null or invalid parameters");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and PageNumber/PageSize must be greater than 0"));
            }
            try
            {
                request.PageSize = Math.Min(request.PageSize, 100); 
                var result = await _roomService.GetPublicRoomsAsync(request.Page, context.CancellationToken, request.PageSize);

                if (result.IsSuccess)
                {
                    var mappedResponse = result.Data?.Select(r => new GrpcRoom
                    {
                        RoomId = r.RoomId ?? string.Empty,
                        RoomName = r.RoomName ?? string.Empty,
                        MaxOccupancy = r.MaxOccupancy,
                        HostId = r.HostId ?? string.Empty,
                        ParticipantIds = { r.ParticipantIds },
                        CurrentVideoId = r.CurrentVideoId ?? string.Empty,
                        CurrentPlaybackPosition = r.CurrentPlaybackPosition,
                        IsPaused = r.IsPaused,
                        LastSyncUpdate = Timestamp.FromDateTime(r.LastSyncUpdate.ToUniversalTime()),
                        IsPrivate = r.IsPrivate,
                        CreatedAt = Timestamp.FromDateTime(r.CreatedAt.ToUniversalTime()),
                    });

                    return new GetPublicRoomsResponse
                    {
                        Rooms = { mappedResponse },
                        TotalCount = mappedResponse?.Count() ?? 0
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve public rooms: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPublicRooms");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GenerateNewRoomPassResponse> GenerateNewRoomPass(GenerateNewRoomPassRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("GenerateNewRoomPassRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.GenerateNewRoomPassAsync(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new GenerateNewRoomPassResponse
                    {
                        NewAccessCode = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to generate new room pass: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateNewRoomPass");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetAccessCodeResponse> GetAccessCode(GetAccessCodeRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("GetAccessCodeRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.GetAccessCode(request.RoomId, request.HostId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new GetAccessCodeResponse
                    {
                        AccessCode = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve access code: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessCode");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetRoomByIdResponse> GetRoomById(GetRoomByIdRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RoomId))
            {
                _logger.LogWarning("GetRoomByIdRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.GetRoomByIdsync(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new InvalidOperationException("GetRoomById returned null data");
                    return new GetRoomByIdResponse
                    {
                        Room = new GrpcRoom
                        {
                            RoomId = resultData.RoomId ?? string.Empty,
                            RoomName = resultData.RoomName ?? string.Empty,
                            MaxOccupancy = resultData.MaxOccupancy,
                            HostId = resultData.HostId ?? string.Empty,
                            ParticipantIds = { resultData.ParticipantIds ?? new List<string>() },
                            CurrentVideoId = resultData.CurrentVideoId ?? string.Empty,
                            CurrentPlaybackPosition =resultData.CurrentPlaybackPosition,
                            IsPaused =resultData.IsPaused,
                            LastSyncUpdate = Timestamp.FromDateTime(resultData.LastSyncUpdate.ToUniversalTime()),
                            IsPrivate = resultData.IsPrivate,
                            CreatedAt = Timestamp.FromDateTime(resultData.CreatedAt.ToUniversalTime()),
                        }
                    };

                }
                else
                {
                    _logger.LogWarning("Failed to retrieve room by ID: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRoomById");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<UpdateRoomResponse> UpdateRoom(UpdateRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("UpdateRoomRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var updateRoomRequest = new Domain.Requests.UpdateRoom_Req
                {
                    RoomId = request.RoomId,
                    RoomName = request.RoomName,
                    MaxOccupancy = request.MaxOccupancy,
                    HostId = request.HostId,

                };

                var result = await _roomService.UpdateRoomAsync(updateRoomRequest, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new InvalidOperationException("UpdateRoom returned null data");
                    return new UpdateRoomResponse
                    {
                        Room = new GrpcRoom()
                        {
                            RoomId = resultData.RoomId ?? string.Empty,
                            RoomName = resultData.RoomName ?? string.Empty,
                            MaxOccupancy = resultData.MaxOccupancy ,
                            HostId = resultData.HostId ?? string.Empty,
                            ParticipantIds = { resultData.ParticipantIds ?? new List<string>() },
                            CurrentVideoId = resultData.CurrentVideoId ?? string.Empty,
                            CurrentPlaybackPosition = resultData.CurrentPlaybackPosition,
                            IsPaused = resultData.IsPaused,
                            LastSyncUpdate = Timestamp.FromDateTime(resultData.LastSyncUpdate.ToUniversalTime()),
                            IsPrivate = resultData.IsPrivate,
                            CreatedAt = Timestamp.FromDateTime(resultData.CreatedAt.ToUniversalTime())

                        }
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to update room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }


        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId) || string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogWarning("JoinRoomRequest is null or RoomId/UserId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId/UserId must be provided"));
            }
            try
            {
                var result = await _roomService.JoinRoomAsync(request.RoomId, request.UserId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new JoinRoomResponse
                    {
                        Success = true,

                    };
                }
                else
                {
                    _logger.LogWarning("Failed to join room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId) || string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogWarning("LeaveRoomRequest is null or RoomId/UserId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId/UserId must be provided"));
            }
            try
            {
                var result = await _roomService.LeaveRoomAsync(request.RoomId, request.UserId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new LeaveRoomResponse
                    {
                        Success = true,

                    };
                }
                else
                {
                    _logger.LogWarning("Failed to leave room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetRoomsByHostResponse> GetRoomsByHost(GetRoomsByHostRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.HostId))
            {
                _logger.LogWarning("Request is null or hostId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and hostId must be provided"));

            }
            try
            {
                var result = await _roomService.GetRoomsByHostAsync(request.HostId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new InvalidOperationException("GetRoomsByHost returned null data");

                    var response = resultData.Select(room => new GrpcRoom
                    {
                        RoomId = room.RoomId ?? string.Empty,
                        RoomName = room.RoomName ?? string.Empty,
                        MaxOccupancy = room.MaxOccupancy,
                        HostId = room.HostId ?? string.Empty,
                        ParticipantIds = { room.ParticipantIds ?? new List<string>() },
                        CurrentVideoId = room.CurrentVideoId ?? string.Empty,
                        CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                        IsPaused = room.IsPaused,
                        LastSyncUpdate = Timestamp.FromDateTime(room.LastSyncUpdate.ToUniversalTime()),
                        IsPrivate = room.IsPrivate,
                        CreatedAt = Timestamp.FromDateTime(room.CreatedAt.ToUniversalTime())

                    });

                    return new GetRoomsByHostResponse
                    {
                        Rooms = { response ?? Enumerable.Empty<GrpcRoom>() }
                    };
                }

                else
                {
                    _logger.LogWarning("Failed to get rooms by hostId: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting room by hostId");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<IsRoomFullResponse> IsRoomFull(IsRoomFullRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("IsRoomFullRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.IsRoomFullAsync(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new IsRoomFullResponse
                    {
                        IsFull = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to check if room is full: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsRoomFull");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<IsUserInRoomResponse> IsUserInRoom(IsUserInRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId) || string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogWarning("IsUserInRoomRequest is null or RoomId/UserId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId/UserId must be provided"));
            }
            try
            {
                var result = await _roomService.IsUserInRoomAsync(request.RoomId, request.UserId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new IsUserInRoomResponse
                    {
                        IsInRoom = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to check if user is in room: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in isUserInRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<MakeRoomPrivateResponse> MakeRoomPrivate(MakeRoomPrivateRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("MakeRoomPrivateRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.MakeRoomPrivateAsync(request.RoomId, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new MakeRoomPrivateResponse
                    {
                        Success = true,
                        NewAccessCode = result.Data ?? string.Empty
                    };
                    
                }
                else
                {
                    _logger.LogWarning("Failed to make room private: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MakeRoomPrivate");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<MakeRoomPublicResponse> MakeRoomPublic(MakeRoomPublicRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("MakeRoomPublicRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.MakeRoomPublicAsync(request.RoomId, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new MakeRoomPublicResponse
                    {
                        Success = true
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to make room public: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MakeRoomPublic");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<RoomExistsResponse> RoomExists(RoomExistsRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("RoomExistsRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.RoomExistsAsync(request.RoomId, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new RoomExistsResponse
                    {
                        Exists = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to check if room exists: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RoomExists");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<UpdateCurrentVideoIdResponse> UpdateCurrentVideoId(UpdateCurrentVideoIdRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("UpdateCurrentVideoIdRequest is null or RoomId/NewVideoId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId/NewVideoId must be provided"));
            }
            try
            {
                var result = await _roomService.UpdateCurrentVideoIdAsync(request.RoomId, request.NewVideoId, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new UpdateCurrentVideoIdResponse
                    {
                        Success = true,

                    };
                }
                else
                {
                   
                    _logger.LogWarning("Failed to update current video ID: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCurrentVideoId");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<UpdatePlaybackStateResponse> UpdatePlaybackState(UpdatePlaybackStateRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId))
            {
                _logger.LogWarning("UpdatePlaybackStateRequest is null or RoomId is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId must be provided"));
            }
            try
            {
                var result = await _roomService.UpdatePlaybackStateAsync(request.RoomId, request.Position, request.IsPaused, request.VideoId, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new UpdatePlaybackStateResponse
                    {
                        Success = true,

                    };
                }
                else
                {
                    _logger.LogWarning("Failed to update playback state: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePlaybackState");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }


        public override async Task<GetPlaybackStateResponse> GetPlaybackState(GetRoomByIdRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.RoomId))
            {
                _logger.LogWarning("GetPlaybackStateForRoom called with null or empty roomId");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "RoomId cannot be null or empty"));
            }

            try
            {
                var result = await _roomService.GetPlaybackStateForRoom(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new RpcException(new Status(StatusCode.NotFound, "Playback state not found for the specified room"));

                    var getPlayBackStateResponse = new GetPlaybackStateResponse()
                    {
                        CurrentPlaybackPosition = resultData.Position,
                        IsPaused = resultData.IsPaused,
                        RoomId = request.RoomId,
                        CurrentVideoId = resultData.VideoId ?? string.Empty,
                        LastSyncUpdate = Timestamp.FromDateTime(resultData.LastSyncUpdate.ToUniversalTime())


                    };

                    return getPlayBackStateResponse;
                }
                else
                {
                    _logger.LogWarning("Failed to get playback state: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPlaybackStateForRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }


        

        public override async Task<ValidateRoomPassResponse> ValidateRoomPass(ValidateRoomPassRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrEmpty(request.RoomId) || string.IsNullOrEmpty(request.AccessCode))
            {
                _logger.LogWarning("ValidateRoomPassRequest is null or RoomId/AccessCode is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and RoomId/AccessCode must be provided"));
            }
            try
            {
                var result = await _roomService.ValidateRoomPassAsync(request.RoomId, request.AccessCode, context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new ValidateRoomPassResponse
                    {
                        IsValid = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to validate room pass: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateRoomPass");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<SearchRoomResponse> SearchRoom(SearchRoomRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                _logger.LogWarning("SearchRoomRequest is null or SearchTerm is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null and SearchTerm must be provided"));
            }

            if (request.Page < 1 || request.PageSize < 1)
            {
                _logger.LogWarning("SearchRoomRequest has invalid Page/PageSize");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0"));
            }
            try
            {
                var thePageSize = Math.Min(request.PageSize, 100);
                var result = await _roomService.SearchRoomsAsync(request.SearchTerm, context.CancellationToken, request.Page, thePageSize);

                if (result.IsSuccess)
                {
                    var resultData = result.Data ?? throw new InvalidOperationException("SearchRooms returned null data");

                    var mappedResponse =resultData.Select(r => new GrpcRoom
                    {
                        RoomId = r.RoomId ?? string.Empty,
                        RoomName = r.RoomName ?? string.Empty,
                        MaxOccupancy = r.MaxOccupancy,
                        HostId = r.HostId ?? string.Empty,
                        ParticipantIds = { r.ParticipantIds ?? new List<string>() },
                        CurrentVideoId = r.CurrentVideoId ?? string.Empty,
                        CurrentPlaybackPosition = r.CurrentPlaybackPosition,
                        IsPaused = r.IsPaused,
                        LastSyncUpdate = Timestamp.FromDateTime(r.LastSyncUpdate.ToUniversalTime()),
                        IsPrivate = r.IsPrivate,
                        CreatedAt = Timestamp.FromDateTime(r.CreatedAt.ToUniversalTime())
                    });

                    return new SearchRoomResponse
                    {
                        Rooms = { mappedResponse ?? Enumerable.Empty<GrpcRoom>() },

                    };
                }
                else
                {
                    _logger.LogWarning("Failed to search rooms: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchRoom");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }


}
