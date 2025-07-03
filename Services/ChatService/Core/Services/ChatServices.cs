using ChatService.Core.Interfaces;
using ChatService.Data.Interrfaces;
using ChatService.Domain.DTO_s;
using ChatService.Domain.Entities;
using ChatService.Domain.Requests;
using ChatService.Domain.UtilModels;
using MongoDB.Bson;

namespace ChatService.Core.Services
{
    public class ChatServices : IChatServices
    {
        private readonly IChatRepo _chatRepo;
        private readonly ILogger<ChatServices> _logger;

        public ChatServices(IChatRepo chatRepo, ILogger<ChatServices> logger)
        {
            _chatRepo = chatRepo ?? throw new ArgumentNullException(nameof(chatRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<bool>> AddChatAsync(AddChat_Req req)
        {
            if (req == null)
            {
                return OperationResult<bool>.Failure(false, "Chat cannot be null");
            }

            try
            {
                var chatToAdd = new Chat
                {
                    RoomId = req.RoomId,
                    UserId = req.UserId,
                    Username = req.Username,
                    Message = req.Message,

                };

                var result = await _chatRepo.AddChatAsync(chatToAdd);

                if (result)
                {
                    return OperationResult<bool>.Success(true, "Chat added");
                }
                else
                {
                    return OperationResult<bool>.Failure(false, "Failed to add chat");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding chat: {ChatId}", req.RoomId);
                return OperationResult<bool>.Error(ex, "Error adding chat");
            }
        }

        public async Task<OperationResult<ChatDto>> GetChatByIdAsync(string chatId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                _logger.LogWarning("Chat ID cannot be null or empty");
                return OperationResult<ChatDto>.Failure(null, "Chat ID cannot be null or empty");
            }
            if (!ObjectId.TryParse(chatId, out var objectId))
            {
                _logger.LogWarning("Invalid chat ID format: {ChatId}", chatId);
                return OperationResult<ChatDto>.Failure(null, "Invalid chat ID format");
            }
            try
            {
                var result = await _chatRepo.GetChatByIdAsync(objectId,cancellation);

                if (result != null)
                {
                    var chatDto = MapOneToDto(result);
                    return OperationResult<ChatDto>.Success(chatDto, "Chat retrieved successfully");
                }
                else
                {
                    _logger.LogDebug("Chat not found for ID: {ChatId}", chatId);
                    return OperationResult<ChatDto>.Failure(null, "Chat not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat by ID: {ChatId}", chatId);
                return OperationResult<ChatDto>.Error(ex, "Error retrieving chat by ID");
            }
        }

        public async Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("Room ID cannot be null or empty");
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "Room ID cannot be null or empty");
            }
            try
            {
                var result = await _chatRepo.GetChatsByRoomIdAsync(roomId, cancellation);

                if (result != null && result.Any())
                {
                    var chatDtos = MapManyToDto(result);

                    chatDtos = chatDtos.OrderByDescending(c => c.Timestamp).ToList();

                    return OperationResult<IEnumerable<ChatDto>>.Success(chatDtos, "Chats retrieved successfully");
                }
                else
                {
                    _logger.LogDebug("No chats found for Room ID: {RoomId}", roomId);
                    return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found for the specified Room ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by Room ID: {RoomId}", roomId);
                return OperationResult<IEnumerable<ChatDto>>.Error(ex, "Error retrieving chats by Room ID");
            }
        }

        public async Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByUserIdAsync(string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("User ID cannot be null or empty");
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "User ID cannot be null or empty");
            }
            try
            {
                var result = await _chatRepo.GetChatsByUserIdAsync(userId, cancellation);

                if (result != null && result.Any())
                {
                    var chatDtos = MapManyToDto(result);
                    chatDtos = chatDtos.OrderByDescending(c => c.Timestamp).ToList();
                    return OperationResult<IEnumerable<ChatDto>>.Success(chatDtos, "Chats retrieved successfully");
                }
                else
                {
                    _logger.LogDebug("No chats found for User ID: {UserId}", userId);
                    return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found for the specified User ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by User ID: {UserId}", userId);
                return OperationResult<IEnumerable<ChatDto>>.Error(ex, "Error retrieving chats by User ID");
            }
        }

        public async Task<OperationResult<string>> GetUserIdFromChat(string chatId, CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(chatId))
            {
                _logger.LogWarning("Chat ID cannot be null or empty");
                return OperationResult<string>.Failure(null, "Chat ID cannot be null or empty");
            }
            try
            {
                var result = await _chatRepo.GetUserIdFromChat(chatId, cancellation);

                if (result != null)
                {
                    return OperationResult<string>.Success(result, "User ID retrieved successfully");
                }
                else
                {
                    _logger.LogDebug("No User ID found for Chat ID: {ChatId}", chatId);
                    return OperationResult<string>.Failure(null, "No User ID found for the specified Chat ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving User ID from chat: {ChatId}", chatId);
                return OperationResult<string>.Error(ex, "Error retrieving User ID from chat");
            }
        }

        private ChatDto MapOneToDto(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "ChatDto cannot be null");
            }

            try
            {
                return new ChatDto
                {
                    Id = chat.Id,
                    RoomId = chat.RoomId,
                    UserId = chat.UserId,
                    Username = chat.Username,
                    Message = chat.Message,
                    Timestamp = chat.Timestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping ChatDto to ChatDto: {ChatId}", chat.Id);
                throw;
            }
        }

        private List<ChatDto> MapManyToDto(IEnumerable<Chat> chats)
        {
            if (chats == null)
            {
                throw new ArgumentNullException(nameof(chats), "Chats cannot be null or empty");
            }

            try
            {
                return chats.Select(MapOneToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping multiple ChatDto to ChatDto");
                throw;
            }
        }
    }
}
