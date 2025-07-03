namespace UserService.Domain.Entities
{
    public class AnonUsers
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        public string UserName { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public string CurrentRoomId { get; set; } = null!;
    }
}
