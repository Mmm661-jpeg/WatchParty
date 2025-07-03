using Grpc.Core;
using SharedProtos;
using SyncService.Core.Interfaces;
using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.ChatRequests;
using SyncService.Domain.UtilModels;
using System.Text.RegularExpressions;

namespace SyncService.Core.Services
{
    public class ChatSyncService : IChatSyncService
    {
        private readonly ILogger<ChatSyncService> _logger;
        private readonly ChatService.ChatServiceClient _chatServiceClient;

        public ChatSyncService(ILogger<ChatSyncService> logger, ChatService.ChatServiceClient chatServiceClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatServiceClient = chatServiceClient ?? throw new ArgumentNullException(nameof(chatServiceClient));
        }

        public async Task<OperationResult<bool>> AddChatAsync(AddChat_Req req, CancellationToken cancellation)
        {
            if (req == null)
            {
                return OperationResult<bool>.Failure(false, "Request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(req.RoomId) || string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Message))
            {
                return OperationResult<bool>.Failure(false, "All fields are required and cannot be empty.");
            }

            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    RoomId = req.RoomId,
                    UserId = req.UserId,
                    Username = req.Username,
                    Message = req.Message
                };

                var grpcResponse = await _chatServiceClient.SendMessageAsync(sendMessageRequest, cancellationToken: cancellation);
                if (grpcResponse.Success)
                {
                    _logger.LogInformation("Chat added successfully for RoomId: {RoomId}, UserId: {UserId}", req.RoomId, req.UserId);
                    return OperationResult<bool>.Success(true, grpcResponse.Message);
                }
                else
                {
                    _logger.LogWarning("Failed to add chat for RoomId: {RoomId}, UserId: {UserId}", req.RoomId, req.UserId);
                    return OperationResult<bool>.Failure(false, grpcResponse.Message);
                }

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to add chat for RoomId: {RoomId}, UserId: {UserId}. Message: {Message}", req.RoomId, req.UserId, errorMessage);
                return OperationResult<bool>.Failure(false, "Failed to add chat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding chat");
                return OperationResult<bool>.Error(ex, "An error occurred while adding the chat.");
            }
        }

        public async Task<OperationResult<ChatDto>> GetChatByIdAsync(string chatId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                return OperationResult<ChatDto>.Failure(null, "Chat ID cannot be null or empty.");
            }

            try
            {
                var getChatRequest = new GetChatByIdRequest { ChatId = chatId };
                var grpcResponse = await _chatServiceClient.GetChatByIdAsync(getChatRequest, cancellationToken: cancellation);

                if (grpcResponse != null)
                {
                    var chatDto = new ChatDto
                    {
                        Id = grpcResponse.Id,
                        RoomId = grpcResponse.RoomId,
                        UserId = grpcResponse.UserId,
                        Username = grpcResponse.Username,
                        Message = grpcResponse.Message,
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(grpcResponse.Timestamp).UtcDateTime,
                    };


                    return OperationResult<ChatDto>.Success(chatDto, "Chat retrieved successfully.");
                }
                else
                {
                    _logger.LogWarning("No chat found for ChatId: {ChatId}", chatId);
                    return OperationResult<ChatDto>.Failure(null, "No chat found with the provided ID.");
                }
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Chat not found for ChatId: {ChatId}. Message: {Message}", chatId,errorMessage);
               
                return OperationResult<ChatDto>.Failure(null, "Chat not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat by ID: {ChatId}", chatId);
                return OperationResult<ChatDto>.Error(ex, "An error occurred while retrieving the chat.");
            }
        }

        public async Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "Room ID cannot be null or empty.");
            }

            try
            {
                var getChatsRequest = new ReceiveMessagesRequest { RoomId = roomId };

                using var call = _chatServiceClient.ReceiveMessages(getChatsRequest, cancellationToken: cancellation);

                var chatDtos = new List<ChatDto>();

                while (await call.ResponseStream.MoveNext(cancellation))
                {
                    var chat = call.ResponseStream.Current;
                    var chatDto = new ChatDto
                    {
                        Id = chat.Id,
                        RoomId = chat.RoomId,
                        UserId = chat.UserId,
                        Username = chat.Username,
                        Message = chat.Message,
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(chat.Timestamp).UtcDateTime
                    };
                    chatDtos.Add(chatDto);
                }

                if (chatDtos.Any())
                {
                    _logger.LogInformation("Chats retrieved successfully for RoomId: {RoomId}", roomId);
                    return OperationResult<IEnumerable<ChatDto>>.Success(chatDtos, "Chats retrieved successfully.");
                }
                else
                {
                    _logger.LogWarning("No chats found for RoomId: {RoomId}", roomId);
                    return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found for the provided Room ID.");
                }
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("No chats found for RoomId: {RoomId}. Message: {Message}", roomId, errorMessage);
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by RoomId: {RoomId}", roomId);
                return OperationResult<IEnumerable<ChatDto>>.Error(ex, "An error occurred while retrieving chats by RoomId.");
            }
        }

        public async Task<OperationResult<IEnumerable<ChatDto>>> GetChatsByUserIdAsync(string userId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "User ID cannot be null or empty.");
            }
            try
            {
                var receiveMessagesRequest = new GetChatHistoryRequest { UserId = userId };

                var grpcResponse = await _chatServiceClient.GetChatHistoryAsync(receiveMessagesRequest, cancellationToken: cancellation);

                if (grpcResponse.Messages.Any())
                {
                    var chatDtos = grpcResponse.Messages.Select(chat => new ChatDto
                    {
                        Id = chat.Id,
                        RoomId = chat.RoomId,
                        UserId = chat.UserId,
                        Username = chat.Username,
                        Message = chat.Message,
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(chat.Timestamp).UtcDateTime
                    }).ToList();

                    _logger.LogInformation("Chats retrieved successfully for UserId: {UserId}", userId);
                    return OperationResult<IEnumerable<ChatDto>>.Success(chatDtos, "Chats retrieved successfully.");
                }
                else
                {
                    _logger.LogWarning("No chats found for UserId: {UserId}", userId);
                    return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found for the provided User ID.");
                }
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("No chats found for UserId: {UserId}. Message: {Message}", userId, errorMessage);
                return OperationResult<IEnumerable<ChatDto>>.Failure(null, "No chats found ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by UserId: {UserId}", userId);
                return OperationResult<IEnumerable<ChatDto>>.Error(ex, "An error occurred while retrieving chats by UserId.");
            }
        }

        public async Task<OperationResult<string>> GetUserIdFromChat(string chatId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                return OperationResult<string>.Failure(null, "Chat ID cannot be null or empty.");
            }
            try
            {
                var getUserIdRequest = new GetChatByIdRequest { ChatId = chatId };

                var grpcResponse = await _chatServiceClient.GetUserIdFromChatAsync(getUserIdRequest, cancellationToken: cancellation);

                if (grpcResponse != null && !string.IsNullOrWhiteSpace(grpcResponse.UserId))
                {
                    _logger.LogInformation("UserId retrieved successfully for ChatId: {ChatId}", chatId);
                    return OperationResult<string>.Success(grpcResponse.UserId, "UserId retrieved successfully.");
                }
                else
                {
                    _logger.LogWarning("No UserId found for ChatId: {ChatId}", chatId);
                    return OperationResult<string>.Failure(null, "No UserId found for the provided Chat ID.");
                }
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("No UserId found. Message: {Message}", errorMessage);
                return OperationResult<string>.Failure(null, "No UserId found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UserId from chat: {ChatId}", chatId);
                return OperationResult<string>.Error(ex, "An error occurred while retrieving UserId from chat.");
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
