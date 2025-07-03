using ChatService.Data.DataModels;
using ChatService.Data.Interrfaces;
using ChatService.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatService.Data.Repos
{
    public class ChatRepo:IChatRepo
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<ChatRepo> _logger;

        public ChatRepo(IDbContext dbContext, ILogger<ChatRepo> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> AddChatAsync(Chat chat)
        {
            try
            {
                var collection = _dbContext.Chats;

                await collection.InsertOneAsync(chat);

             

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error adding chat: {ChatId}", chat.Id);
                throw;
            }
        }

        public async Task<Chat> GetChatByIdAsync(ObjectId chatId, CancellationToken cancellation)
        {
           
            try
            {
                var collection = _dbContext.Chats;
                var filter = Builders<Chat>.Filter.Eq("_id", chatId);
                
                var chat = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

              

               
                return chat;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat by ID: {ChatId}", chatId);
                throw;
            }
        }

        public async Task<IEnumerable<Chat>> GetChatsByRoomIdAsync(string roomId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Chats;

                var filter = Builders<Chat>.Filter.Eq(c => c.RoomId, roomId);

                var chats = await collection.Find(filter).ToListAsync(cancellation);
               

                return chats;


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by room ID: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Chats;

                var filter = Builders<Chat>.Filter.Eq(c => c.UserId, userId);

                var chats = await collection.Find(filter).ToListAsync(cancellation);

              
                return chats;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats by user ID: {UserId}", userId);
                throw;
            }
        }


        public async Task<string> GetUserIdFromChat(string chatId, CancellationToken cancellation)
        {
            try
            {
                var collection = _dbContext.Chats;

                var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);

                var chat = await collection.Find(filter).FirstOrDefaultAsync(cancellation);

                if (chat == null)
                {
                   
                    return null;
                }

                return chat.UserId;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user ID from chat: {ChatId}", chatId);
                throw;
            }
        }
    }
}
