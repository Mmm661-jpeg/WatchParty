using ChatService.Core.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Requests;
using Grpc.Core;
using SharedProtos;

namespace ChatService.Core.Grpc
{
    public class ChatGrpcService : SharedProtos.ChatService.ChatServiceBase
    {
        private readonly IChatServices _chatService;
        private readonly ILogger<ChatGrpcService> _logger;

        public ChatGrpcService(IChatServices chatService, ILogger<ChatGrpcService> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public override async Task<ChatMessage> GetChatById(GetChatByIdRequest request, ServerCallContext context)
        {
            try
            {
                var result = await _chatService.GetChatByIdAsync(request.ChatId,cancellation:context.CancellationToken);

                if (result.IsSuccess)
                {
                    return new ChatMessage
                    {
                        Id = result.Data?.Id,
                        RoomId = result.Data?.RoomId,
                        Username = result.Data?.Username,
                        UserId = result.Data?.UserId,
                        Message = result.Data?.Message,
                        Timestamp = new DateTimeOffset(result.Data.Timestamp).ToUnixTimeMilliseconds()
                    };

                }
                else
                {
                    _logger.LogWarning("Chat not found for ID: {ChatId}", request.ChatId);
                    throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
                }
            }
            catch (RpcException)
            {
               
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatById");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetChatHistoryResponse> GetChatHistory(GetChatHistoryRequest request, ServerCallContext context)
        {
            try
            {
                var result = await _chatService.GetChatsByUserIdAsync(request.UserId, cancellation: context.CancellationToken);

                if(result.IsSuccess)
                {
                    var response =  result.Data?.Select(chat => new ChatMessage
                    {
                        Id = chat.Id,
                        RoomId = chat.RoomId,
                        Username = chat.Username,
                        UserId = chat.UserId,
                        Message = chat.Message,
                        Timestamp = new DateTimeOffset(chat.Timestamp).ToUnixTimeMilliseconds()

                    }).ToList();

                    return new GetChatHistoryResponse
                    {
                        Messages = { response }
                    };

                }
                else
                {
                    _logger.LogWarning("No chats found for user ID: {UserId}", request.UserId);
                    throw new RpcException(new Status(StatusCode.NotFound, "No chats found"));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatHistory");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetUserIdFromChatResponse> GetUserIdFromChat(GetChatByIdRequest request, ServerCallContext context)
        {
           try
            {
                var result = await _chatService.GetUserIdFromChat(request.ChatId, cancellation: context.CancellationToken);
                if (result.IsSuccess)
                {
                    return new GetUserIdFromChatResponse
                    {
                        UserId = result.Data
                    };
                }
                else
                {
                    _logger.LogWarning("Chat not found for ID: {ChatId}", request.ChatId);
                    throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserIdFromChat");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task ReceiveMessages(ReceiveMessagesRequest request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            try
            {
                var result = await _chatService.GetChatsByRoomIdAsync(request.RoomId, cancellation: context.CancellationToken);

                if(result.IsSuccess)
                {
                   var messages = result.Data?.Select(chat => new ChatMessage
                   {
                       Id = chat.Id,
                       RoomId = chat.RoomId,
                       UserId = chat.UserId,
                       Username = chat.Username,
                       Message = chat.Message,
                       Timestamp = new DateTimeOffset(chat.Timestamp).ToUnixTimeMilliseconds()
                   });

                    if (messages != null)
                    {
                        foreach (var message in messages)
                        {
                            await responseStream.WriteAsync(message);

                            if (context.CancellationToken.IsCancellationRequested)
                                break;
                        }
                    }


                }
                else
                {
                    _logger.LogWarning("No messages found for room ID: {RoomId}", request.RoomId);
                    throw new RpcException(new Status(StatusCode.NotFound, "No messages found"));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReceiveMessages");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            try
            {
                var chat = new AddChat_Req
                {
                    
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    Username = request.Username,
                    Message = request.Message,
                    
                };

               
                

                var result = await _chatService.AddChatAsync(chat);

                if (result.IsSuccess)
                {
                    return new SendMessageResponse
                    {
                        Success = true,
                        Message = result.Message
                    };
                }
                else
                {
                   
                    _logger.LogWarning("Failed to send message: {Error}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to send message"));
                }


            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
