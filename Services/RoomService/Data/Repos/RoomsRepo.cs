using MongoDB.Driver;
using RoomService.Data.DataModels;
using RoomService.Data.Interfaces;
using RoomService.Domain.Entities;

namespace RoomService.Data.Repos
{
    public class RoomsRepo : IRoomsRepo
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<RoomsRepo> _logger;

        public RoomsRepo(IDbContext dbContext, ILogger<RoomsRepo> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Rooms> CreateRoomAsync(Rooms room,CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var roomNameIsUnique = await collection.Find(r => r.RoomName == room.RoomName)
                    .AnyAsync(cancellationToken: cancellation);

                if (roomNameIsUnique)
                {
                    _logger.LogWarning("Room name {RoomName} already exists", room.RoomName);
                    return null;
                }


                    await collection.InsertOneAsync(room,cancellationToken:cancellation);

                _logger.LogInformation("Room {RoomId} created successfully", room.RoomId);
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room {RoomId}", room.RoomId);
                throw;
            }
        }

        public async Task<bool> DeleteRoomAsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var result = await collection.DeleteOneAsync(filter,cancellationToken:cancellation);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Room {RoomId} deleted successfully", roomId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found for deletion", roomId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<string> GenerateNewRoomPassAsync(string roomId, string newPass, CancellationToken cancellation)
        {
            try
            {


                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And
                    (
                        Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                        Builders<Rooms>.Filter.Eq(r => r.IsPrivate, true) // Ensure only private rooms can have passcodes
                    );

                var update = Builders<Rooms>.Update.Set(r => r.AccessCode, newPass);

                var result = await collection.UpdateOneAsync(filter, update,cancellationToken:cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("New passcode generated for room {RoomId}", roomId);
                    return newPass;
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found for passcode generation", roomId);
                    return null;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating new room pass for {RoomId}", roomId);
                throw;
            }
        }

        public async Task<List<Rooms>> GetAllRoomsAsync(CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                return await collection.Find(_ => true).ToListAsync(cancellation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rooms");
                throw;
            }
        }

        public async Task<List<Rooms>> GetPrivateRoomsAsync(CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.IsPrivate, true);

                return await collection.Find(filter).ToListAsync(cancellation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private rooms");
                throw;
            }
        }

        public async Task<List<Rooms>> GetPublicRoomsAsync(int page, CancellationToken cancellationn, int pagesize)
        {
            try
            {
                var collection = _dbContext.Rooms;
                var filter = Builders<Rooms>.Filter.Eq(r => r.IsPrivate, false);

                return await collection.Find(filter).Skip((page - 1) * pagesize).Limit(pagesize).ToListAsync(cancellationn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public rooms");
                throw;
            }
        }

        public async Task<Rooms> GetRoomByIdsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var room = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room by ID {RoomId}", roomId);
                throw;
            }
        }

        public async Task<List<Rooms>> GetRoomsByHostAsync(string hostId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.HostId, hostId);

                return await collection.Find(filter).ToListAsync(cancellation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms by host ID {HostId}", hostId);
                throw;
            }
        }

        public async Task<bool> JoinRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var room = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

                if (room == null)
                {
                    _logger.LogWarning($"Room {roomId} not found");
                    return false;
                }


                if (room.ParticipantIds.Contains(userId))
                {
                    _logger.LogWarning("User {UserId} is already in room {RoomId}", userId, roomId);
                    return true;
                }

                if (room.ParticipantIds.Count >= room.MaxOccupancy)
                {
                    _logger.LogWarning("Room {RoomId} is full, cannot join", roomId);
                    return false;
                }


                var update = Builders<Rooms>.Update.Push(r => r.ParticipantIds, userId);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("User {UserId} joined room {RoomId} successfully", userId, roomId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to join room {RoomId} - room may not exist or user already in room", userId, roomId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room {RoomId} for user {UserId}", roomId, userId);
                throw;
            }
        }

        public async Task<bool> LeaveRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var update = Builders<Rooms>.Update.Pull(r => r.ParticipantIds, userId);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("User {UserId} left room {RoomId} successfully", userId, roomId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to leave room {RoomId} - room may not exist or user not in room", userId, roomId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room {RoomId} for user {UserId}", roomId, userId);
                throw;
            }
        }

        public async Task<bool> ValidateRoomPassAsync(string roomId, string accessCode, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And(
                    Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                    Builders<Rooms>.Filter.Eq(r => r.AccessCode, accessCode)
                );

                var exists = await collection.Find(filter).AnyAsync(cancellation);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating room pass for {RoomId}", roomId);
                throw;
            }
        }

        public async Task<bool> RoomExistsAsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var exists = await collection.Find(filter).AnyAsync(cancellation);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room exists {RoomId}", roomId);
                throw;
            }
        }

        public Task<List<Rooms>> SearchRoomsAsync(string searchTerm, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Or(
                    Builders<Rooms>.Filter.Regex(r => r.RoomName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Rooms>.Filter.Regex(r => r.HostId, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                   
                );

                var result = collection.Find(filter)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync(cancellation);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching rooms with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<bool> UpdatePlaybackStateAsync(string roomId, double position, bool isPaused, string videoId,CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And(
                            Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                            Builders<Rooms>.Filter.Eq(r => r.CurrentVideoId, videoId)
                );

                var update = Builders<Rooms>.Update
                    .Set(r => r.CurrentPlaybackPosition, position)
                    .Set(r => r.IsPaused, isPaused)
                    .Set(r => r.LastSyncUpdate, DateTime.UtcNow);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Playback state updated for room {RoomId}", roomId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update playback state for room {RoomId} - room may not exist", roomId);
                    return false;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playback state for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<Rooms?> UpdateRoomAsync(string roomId, string hostId, string? roomName, int? maxOccupancy, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

              

                var filter = Builders<Rooms>.Filter.And(
                    Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                    Builders<Rooms>.Filter.Eq(r => r.HostId, hostId)
                );

               
                var updates = new List<UpdateDefinition<Rooms>>();

                if (!string.IsNullOrEmpty(roomName))
                {
                    updates.Add(Builders<Rooms>.Update.Set(r => r.RoomName, roomName));
                }

                if (maxOccupancy.HasValue)
                {
                    updates.Add(Builders<Rooms>.Update.Set(r => r.MaxOccupancy, maxOccupancy));
                }

                if (updates.Count == 0)
                {
                    _logger.LogWarning("No updates provided for room {RoomId}", roomId);
                    return null; 
                }

                var combinedUpdate = Builders<Rooms>.Update.Combine(updates);


                var result = await collection.FindOneAndUpdateAsync(filter, combinedUpdate,
                    new FindOneAndUpdateOptions<Rooms>
                    {
                        ReturnDocument = ReturnDocument.After
                    },cancellationToken:cancellation);

                if (result != null)
                {
                    _logger.LogInformation("Room {RoomId} updated successfully", roomId);
                    return result;
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found for update", roomId);
                    return null;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<bool> IsRoomFullAsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var room = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

                if (room == null)
                {
                    _logger.LogWarning("Room {RoomId} not found when checking if full", roomId);
                    return false;
                }

                bool isFull = room.ParticipantIds.Count >= room.MaxOccupancy;
                _logger.LogInformation("Room {RoomId} is {FullStatus}", roomId, isFull ? "full" : "not full");
                return isFull;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room {RoomId} is full", roomId);
                throw;
            }
        }

        public async Task<string> MakeRoomPrivateAsync(string roomId, string newPass,CancellationToken cancellation)
        {
            try
            {

                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And(
                    Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                    Builders<Rooms>.Filter.Eq(r => r.IsPrivate, false)
                );

                var update = Builders<Rooms>.Update
                    .Set(r => r.IsPrivate, true)
                    .Set(r => r.AccessCode, newPass);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Room {RoomId} made private successfully with new access code {AccessCode}", roomId, newPass);
                    return newPass;
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found or already private", roomId);
                    return null;
                }



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room {RoomId} private", roomId);
                throw;
            }
        }

        public async Task<bool> MakeRoomPublicAsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And(
                    Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                    Builders<Rooms>.Filter.Eq(r => r.IsPrivate, true)
                );

                var update = Builders<Rooms>.Update
                    .Set(r => r.IsPrivate, false)
                    .Unset(r => r.AccessCode);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Room {RoomId} made public successfully", roomId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found or already public", roomId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making room {RoomId} public", roomId);
                throw;
            }
        }

        public async Task<bool> IsUserInRoomAsync(string roomId, string userId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.And(
                    Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId),
                     Builders<Rooms>.Filter.AnyEq(r => r.ParticipantIds, userId));

                var exists = await collection.Find(filter).AnyAsync(cancellation);

                _logger.LogInformation("User {UserId} is {InRoomStatus} in room {RoomId}", userId, exists ? "in" : "not in", roomId);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is in room {RoomId}", userId, roomId);
                throw;
            }
        }

        public async Task<string> GetAccessCode(string roomId, string hostId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var room = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

                if (room == null)
                {
                    _logger.LogWarning("Room {RoomId} not found when getting access code", roomId);
                    return null;
                }

                if (room.IsPrivate && !room.HostId.Equals(hostId))
                {
                    _logger.LogWarning("User {UserId} is not a host in private room {RoomId}", hostId, roomId);
                    return null;
                }

                return room.AccessCode ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access code for room {RoomId} and user {UserId}", roomId, hostId);
                throw;
            }
        }

        public async Task<bool> UpdateCurrentVideoIdAsync(string roomId, string videoId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Rooms;

                var filter = Builders<Rooms>.Filter.Eq(r => r.RoomId, roomId);

                var update = Builders<Rooms>.Update
                    .Set(r => r.CurrentVideoId, videoId)
                    .Set(r => r.CurrentPlaybackPosition, 0) 
                    .Set(r => r.IsPaused, true)
                    .Set(r => r.LastSyncUpdate, DateTime.UtcNow);

                var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellation);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Current video ID updated for room {RoomId} to {VideoId}", roomId, videoId);
                    return true; // Return updated room
                }
                else
                {
                    _logger.LogWarning("Room {RoomId} not found or video ID not updated", roomId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current video ID for room {RoomId}", roomId);
                throw;
            }
        }

        private string GenerateRandomAccessCode()
        {
            var newPass = Guid.NewGuid().ToString("N").Substring(0, 6);

            return newPass.ToUpperInvariant();
        }


    }
}
