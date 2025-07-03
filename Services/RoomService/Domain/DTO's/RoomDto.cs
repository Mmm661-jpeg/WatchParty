namespace RoomService.Domain.DTO_s
{
    public class RoomDto
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public int MaxOccupancy { get; set; }
        public string HostId { get; set; }
        public List<string> ParticipantIds { get; set; }
        public string? CurrentVideoId { get; set; }
        public double CurrentPlaybackPosition { get; set; }
        public bool IsPaused { get; set; }
        public DateTime LastSyncUpdate { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
