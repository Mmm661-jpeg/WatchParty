using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ChatService.Domain.DTO_s
{
    public class ChatDto
    {

        public string Id { get; set; }
        public string RoomId { get; set; }


        public string UserId { get; set; }

        
        public string Username { get; set; }

      
        public string Message { get; set; }

       
        public DateTime Timestamp { get; set; } 
    }
}
