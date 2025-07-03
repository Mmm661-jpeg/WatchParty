using RoomService.Core.Interfaces;
using RoomService.Data.Interfaces;
using RoomService.Domain.DTO_s;
using RoomService.Domain.Entities;
using RoomService.Domain.Requests;
using RoomService.Domain.UtilModels;

namespace RoomService.Core.Services
{
    public class RoomsServices : IRoomsServices
    {
        private readonly IRoomsRepo _roomsRepo;
        private readonly ILogger<RoomsServices> _logger;

        public RoomsServices(IRoomsRepo roomsRepo, ILogger<RoomsServices> logger)
        {
            _roomsRepo = roomsRepo ?? throw new ArgumentNullException(nameof(roomsRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<RoomDto>> CreateRoomAsync(CreateRoom_Req req,CancellationToken cancellation)
        {
            if(req == null)
            {
                _logger.LogWarning("CreateRoom_Req cannot be null");
                return OperationResult<RoomDto>.Failure(null, "CreateRoom_Req cannot be null");
            }
            if(string.IsNullOrWhiteSpace(req.RoomName) || string.IsNullOrWhiteSpace(req.HostId))
            {
                _logger.LogWarning("Room name and Host ID cannot be null or empty");
                return OperationResult<RoomDto>.Failure(null, "Room name and Host ID cannot be null or empty");
            }
            if (req.MaxOccupancy <= 0)
            {
                _logger.LogWarning("Max occupancy must be greater than 0");
                return OperationResult<RoomDto>.Failure(null, "Max occupancy must be greater than 0");
            }
            try
            {
                var newRoom = new Rooms()
                {
                    RoomName = req.RoomName,
                    MaxOccupancy = req.MaxOccupancy,
                    HostId = req.HostId,
                    IsPrivate = req.IsPrivate,
                };

                if(req.IsPrivate)
                {
                    newRoom.AccessCode = GenerateRandomAccessCode();
                }

                var createdRoom = await _roomsRepo.CreateRoomAsync(newRoom,cancellation);

                if (createdRoom == null)
                {
                    _logger.LogWarning("Failed to create room: {RoomName}", req.RoomName);
                    return OperationResult<RoomDto>.Failure(null, "Failed to create room");
                }
                var roomDto = MapOneToDto(createdRoom);
                _logger.LogInformation("Successfully created room: {RoomName}", req.RoomName);

                return OperationResult<RoomDto>.Success(roomDto, "Room created successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room: {RoomId}", req.RoomName);
                return OperationResult<RoomDto>.Error(ex, "Error creating room");
            }
        }

        public async Task<OperationResult<bool>> DeleteRoomAsync(string roomId, CancellationToken cancellation) 
        {
            if(string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.DeleteRoomAsync(roomId, cancellation);

                if (!result)
                {
                    _logger.LogWarning("Failed to delete room: {RoomId}", roomId);
                    return OperationResult<bool>.Failure(false, "Failed to delete room");
                }
                _logger.LogInformation("Successfully deleted room: {RoomId}", roomId);
                return OperationResult<bool>.Success(true, "Room deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error deleting room");
            }
        }

        public async Task<OperationResult<string>> GenerateNewRoomPassAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<string>.Failure(null, "Room ID cannot be null or empty");
            }

            try
            {
                var newPass = GenerateRandomAccessCode();

                var result = await _roomsRepo.GenerateNewRoomPassAsync(roomId, newPass,cancellation);

                if (string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogWarning("Failed to generate new room pass for room: {RoomId}", roomId);
                    return OperationResult<string>.Failure(null, "Failed to generate new room pass");
                }

                _logger.LogInformation("Successfully generated new room pass for room: {RoomId}", roomId);
                return OperationResult<string>.Success(result, "New room pass generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating new room pass for room: {RoomId}", roomId);
                return OperationResult<string>.Error(ex, "Error generating new room pass");
            }
        }

        public async Task<OperationResult<string>> GetAccessCode(string roomId, string hostId,CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(hostId))
            {
                _logger.LogError("Room ID and User ID cannot be null or empty");
                return OperationResult<string>.Failure(null, "Room ID and User ID cannot be null or empty");
            }
            try
            {
                var accessCode = await _roomsRepo.GetAccessCode(roomId, hostId, cancellation);

                if (string.IsNullOrWhiteSpace(accessCode))
                {
                    _logger.LogWarning("No access code found for room: {RoomId} and user: {UserId}", roomId, hostId);
                    return OperationResult<string>.Failure(null, "No access code found");
                }

                _logger.LogInformation("Successfully retrieved access code for room: {RoomId} and user: {UserId}", roomId, hostId);
                return OperationResult<string>.Success(accessCode, "Access code retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access code for room: {RoomId} and user: {UserId}", roomId, hostId);
                return OperationResult<string>.Error(ex, "Error getting access code");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetAllRoomsAsync(CancellationToken cancellation)
        {
            try
            {
                var result = await _roomsRepo.GetAllRoomsAsync(cancellation);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No rooms found");
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No rooms found");
                }

                var mappedResult = MapManyToDto(result);

                _logger.LogInformation("Successfully retrieved all rooms");
                return OperationResult<List<RoomDto>>.Success(mappedResult, "All rooms retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rooms");
                return OperationResult<List<RoomDto>>.Error(ex, "Error retrieving all rooms");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetPrivateRoomsAsync(CancellationToken cancellation)
        {
            try
            {
                var result = await _roomsRepo.GetPrivateRoomsAsync(cancellation);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No private rooms found");
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No private rooms found");
                }

                var mappedResult = MapManyToDto(result);

                _logger.LogInformation("Successfully retrieved private rooms");
                return OperationResult<List<RoomDto>>.Success(mappedResult, "Private rooms retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private rooms");
                return OperationResult<List<RoomDto>>.Error(ex, "Error retrieving private rooms");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetPublicRoomsAsync(int page,CancellationToken cancellation,int pageSize = 100)
        {
            if(page < 1 || pageSize < 1)
            {
                _logger.LogError("Page number and page size must be greater than 0");
                return OperationResult<List<RoomDto>>.Failure(null, "Page number and page size must be greater than 0");
            }
            try
            {
                var result = await _roomsRepo.GetPublicRoomsAsync(page,cancellation,pageSize);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No public rooms found");
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No public rooms found");
                }

                var mappedResult = MapManyToDto(result);

                _logger.LogInformation("Successfully retrieved public rooms");
                return OperationResult<List<RoomDto>>.Success(mappedResult, "Public rooms retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public rooms");
                return OperationResult<List<RoomDto>>.Error(ex, "Error retrieving public rooms");
            }

        }

        public async Task<OperationResult<RoomDto>> GetRoomByIdsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<RoomDto>.Failure(null, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.GetRoomByIdsync(roomId, cancellation);

                if (result == null)
                {
                    _logger.LogWarning("Room not found with ID: {RoomId}", roomId);
                    return OperationResult<RoomDto>.Failure(null, "Room not found");
                }
                var mappedResult = MapOneToDto(result);

                _logger.LogInformation("Successfully retrieved room with ID: {RoomId}", roomId);
                return OperationResult<RoomDto>.Success(mappedResult, "Room retrieved successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room by ID: {RoomId}", roomId);
                return OperationResult<RoomDto>.Error(ex, "Error retrieving room by ID");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> GetRoomsByHostAsync(string hostId,CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                _logger.LogError("Host ID cannot be null or empty");
                return OperationResult<List<RoomDto>>.Failure(null, "Host ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.GetRoomsByHostAsync(hostId, cancellation);
                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No rooms found for host ID: {HostId}", hostId);
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No rooms found for host ID");
                }
                var mappedResult = MapManyToDto(result);
                _logger.LogInformation("Successfully retrieved rooms for host ID: {HostId}", hostId);
                return OperationResult<List<RoomDto>>.Success(mappedResult, "Rooms retrieved successfully for host ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms by host ID: {HostId}", hostId);
                return OperationResult<List<RoomDto>>.Error(ex, "Error retrieving rooms by host ID");

            }
        }

        public async Task<OperationResult<bool>> IsRoomFullAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.IsRoomFullAsync(roomId, cancellation);

                if (result)
                {
                    _logger.LogInformation("Room with ID: {RoomId} is full", roomId);
                    return OperationResult<bool>.Success(true, "Room is full");
                }
                else
                {
                    _logger.LogInformation("Room with ID: {RoomId} is not full", roomId);
                    return OperationResult<bool>.Success(false, "Room is not full");
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room is full for room ID: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error checking if room is full");
            }
        }

        public async Task<OperationResult<bool>> IsUserInRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("Room ID and User ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID and User ID cannot be null or empty");
            }

            try
            {
                var result = await _roomsRepo.IsUserInRoomAsync(roomId, userId, cancellation);

                if (result)
                {
                    _logger.LogInformation("User with ID: {UserId} is in room: {RoomId}", userId, roomId);
                    return OperationResult<bool>.Success(true, "User is in room");
                }
                else
                {
                    _logger.LogInformation("User with ID: {UserId} is not in room: {RoomId}", userId, roomId);
                    return OperationResult<bool>.Success(false, "User is not in room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is in room: {RoomId} for user: {UserId}", roomId, userId);
                return OperationResult<bool>.Error(ex, "Error checking if user is in room");
            }
        }

        public async Task<OperationResult<bool>> JoinRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("Room ID and User ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID and User ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.JoinRoomAsync(roomId, userId,cancellation);

                if (result)
                {
                    _logger.LogInformation("User with ID: {UserId} successfully joined room: {RoomId}", userId, roomId);
                    return OperationResult<bool>.Success(true, "User joined room successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to join room: {RoomId} for user: {UserId}", roomId, userId);
                    return OperationResult<bool>.Failure(false, "Failed to join room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room: {RoomId} for user: {UserId}", roomId, userId);
                return OperationResult<bool>.Error(ex, "Error joining room");
            }
        }

        public async Task<OperationResult<bool>> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("Room ID and User ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID and User ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.LeaveRoomAsync(roomId, userId, cancellation);
                if (result)
                {
                    _logger.LogInformation("User with ID: {UserId} successfully left room: {RoomId}", userId, roomId);
                    return OperationResult<bool>.Success(true, "User left room successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to leave room: {RoomId} for user: {UserId}", roomId, userId);
                    return OperationResult<bool>.Failure(false, "Failed to leave room");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room: {RoomId} for user: {UserId}", roomId, userId);
                return OperationResult<bool>.Error(ex, "Error leaving room");
            }
        }

        public async Task<OperationResult<string>> MakeRoomPrivateAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<string>.Failure(null, "Room ID cannot be null or empty");
            }
            try
            {
                var newPass = GenerateRandomAccessCode();

                var result = await _roomsRepo.MakeRoomPrivateAsync(roomId,newPass, cancellation);

                if (string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogWarning("Failed to make room private: {RoomId}", roomId);
                    return OperationResult<string>.Failure(null, "Failed to make room private");
                }

                _logger.LogInformation("Successfully made room private: {RoomId}", roomId);
                return OperationResult<string>.Success(result, "Room made private successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room private: {RoomId}", roomId);
                return OperationResult<string>.Error(ex, "Error making room private");
            }
        }

        public async Task<OperationResult<bool>> MakeRoomPublicAsync(string roomId, CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.MakeRoomPublicAsync(roomId, cancellation);

                if (!result)
                {
                    _logger.LogWarning("Failed to make room public: {RoomId}", roomId);
                    return OperationResult<bool>.Failure(false, "Failed to make room public");
                }
                _logger.LogInformation("Successfully made room public: {RoomId}", roomId);
                return OperationResult<bool>.Success(true, "Room made public successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room public: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error making room public");
            }
        }

        public async Task<OperationResult<bool>> RoomExistsAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.RoomExistsAsync(roomId, cancellation);

                if (result)
                {
                    _logger.LogInformation("Room exists with ID: {RoomId}", roomId);
                    return OperationResult<bool>.Success(true, "Room exists");
                }
                else
                {
                    _logger.LogInformation("Room does not exist with ID: {RoomId}", roomId);
                    return OperationResult<bool>.Success(false, "Room does not exist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room exists: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error checking if room exists");
            }
        }

        public async Task<OperationResult<List<RoomDto>>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation, int page, int pageSize = 100)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogError("Search term cannot be null or empty");
                return OperationResult<List<RoomDto>>.Failure(null, "Search term cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.SearchRoomsAsync(searchTerm, cancellation,page,pageSize);
                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No rooms found for search term: {SearchTerm}", searchTerm);
                    return OperationResult<List<RoomDto>>.Success(new List<RoomDto>(), "No rooms found");
                }
                var mappedResult = MapManyToDto(result);
                _logger.LogInformation("Successfully searched rooms with term: {SearchTerm}", searchTerm);
                return OperationResult<List<RoomDto>>.Success(mappedResult, "Rooms searched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching rooms with term: {SearchTerm}", searchTerm);
                return OperationResult<List<RoomDto>>.Error(ex, "Error searching rooms");
            }
        }

        public async Task<OperationResult<bool>> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused, string videoId,CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(videoId))
            {
                _logger.LogWarning("Video ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Video ID cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.UpdatePlaybackStateAsync(roomId, position, isPaused,videoId,cancellation);

                if (!result)
                {
                    _logger.LogWarning("Failed to update playback state for room: {RoomId}", roomId);
                    return OperationResult<bool>.Failure(false, "Failed to update playback state");
                }

                _logger.LogInformation("Successfully updated playback state for room: {RoomId}", roomId);
                return OperationResult<bool>.Success(true, "Playback state updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playback state for room: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error updating playback state");
            }
        }

        public async Task<OperationResult<RoomDto>> UpdateRoomAsync(UpdateRoom_Req req,CancellationToken cancellation)
        {
            if (req == null)
            {
                const string message = "UpdateRoom_Req cannot be null";
                _logger.LogError(message);
                return OperationResult<RoomDto>.Failure(null, message);
            }

            if (string.IsNullOrWhiteSpace(req.RoomId))
            {
                const string message = "Room ID cannot be empty";
                _logger.LogError(message);
                return OperationResult<RoomDto>.Failure(null, message);
            }

            if (string.IsNullOrWhiteSpace(req.HostId))
            {
                const string message = "Host ID is required";
                _logger.LogError(message);
                return OperationResult<RoomDto>.Failure(null, message);
            }

            try
            {
              
                string newRoomName = string.IsNullOrWhiteSpace(req.RoomName) ? null : req.RoomName.Trim();
                int? newMaxOccupancy = req.MaxOccupancy > 0 ? req.MaxOccupancy : null;

                
                var updatedRoom = await _roomsRepo.UpdateRoomAsync(req.RoomId,req.HostId,newRoomName,newMaxOccupancy,cancellation);

                if (updatedRoom == null)
                {
                    _logger.LogWarning("Failed to update room: {RoomId}", req.RoomId);
                    return OperationResult<RoomDto>.Failure(null, "Failed to update room");
                }

                var roomDto = MapOneToDto(updatedRoom);

                _logger.LogInformation("Successfully updated room: {RoomId}", req.RoomId);
                return OperationResult<RoomDto>.Success(roomDto, "Room updated successfully");


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating room: {RoomId}", req.RoomId);
                return OperationResult<RoomDto>.Error(ex, "Error updating room");
            }
                
        }

        public async Task<OperationResult<bool>> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(accessCode))
            {
                _logger.LogError("Room ID and Access Code cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID and Access Code cannot be null or empty");
            }
            try
            {
                var result = await _roomsRepo.ValidateRoomPassAsync(roomId, accessCode, cancellation);
                if (!result)
                {
                    _logger.LogWarning("Invalid access code for room: {RoomId}", roomId);
                    return OperationResult<bool>.Failure(false, "Invalid access code");
                }
                _logger.LogInformation("Successfully validated access code for room: {RoomId}", roomId);
                return OperationResult<bool>.Success(true, "Access code validated successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error validating room pass for room: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error validating room pass");
            }
        }

        public async Task<OperationResult<bool>> UpdateCurrentVideoIdAsync(string roomId, string videoId, CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<bool>.Failure(false, "Room ID cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(videoId))
            {
                videoId = null;
            }

            try
            {
                var result = await _roomsRepo.UpdateCurrentVideoIdAsync(roomId, videoId, cancellation);

                if (!result)
                {
                    _logger.LogWarning("Failed to update current video ID for room: {RoomId}", roomId);
                    return OperationResult<bool>.Failure(false, "Failed to update current video ID");
                }

                _logger.LogInformation("Successfully updated current video ID for room: {RoomId}", roomId);
                return OperationResult<bool>.Success(true, "Current video ID updated successfully");
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current video ID for room: {RoomId}", roomId);
                return OperationResult<bool>.Error(ex, "Error updating current video ID");
            }
        }

        public async Task<OperationResult<PlaybackStateDto>> GetPlaybackStateForRoom(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogError("Room ID cannot be null or empty");
                return OperationResult<PlaybackStateDto>.Failure(null, "Room ID cannot be null or empty");
            }

            try
            {
                var room = await _roomsRepo.GetRoomByIdsync(roomId, cancellation);
                if (room == null)
                {
                    _logger.LogWarning("Room not found with ID: {RoomId}", roomId);
                    return OperationResult<PlaybackStateDto>.Failure(null, "Room not found");
                }

                var playbackState = new PlaybackStateDto
                {
                    VideoId = room.CurrentVideoId,
                    Position = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate
                };

                _logger.LogInformation("Successfully retrieved playback state for room: {RoomId}", roomId);
                return OperationResult<PlaybackStateDto>.Success(playbackState, "Playback state retrieved successfully");

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving playback state for room: {RoomId}", roomId);
                return OperationResult<PlaybackStateDto>.Error(ex, "Error retrieving playback state");
            }
        }

            private string GenerateRandomAccessCode()
        {
            var newPass = Guid.NewGuid().ToString("N").Substring(0, 6);

            return newPass.ToUpperInvariant();
        }

        private List<RoomDto> MapManyToDto(List<Rooms> rooms)
        {
            if (rooms == null)
            {

                throw new ArgumentException("Rooms list cannot be null or empty", nameof(rooms));
            }
            try
            {
                return rooms.Select(MapOneToDto).ToList();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error mapping rooms to DTOs");
                throw;
            }
           
        }
        private RoomDto MapOneToDto(Rooms room)
        {
            if(room == null)
            {
                throw new ArgumentNullException(nameof(room), "Room cannot be null");
            }

           try
            {
                return new RoomDto
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    MaxOccupancy = room.MaxOccupancy,
                    HostId = room.HostId,
                    ParticipantIds = room.ParticipantIds,
                    CurrentVideoId = room.CurrentVideoId,
                    CurrentPlaybackPosition = room.CurrentPlaybackPosition,
                    IsPaused = room.IsPaused,
                    LastSyncUpdate = room.LastSyncUpdate,
                    IsPrivate = room.IsPrivate,
                    CreatedAt = room.CreatedAt
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error mapping room to DTO for RoomId: {RoomId}", room.RoomId);
                throw;
            }
        }
    }
}
