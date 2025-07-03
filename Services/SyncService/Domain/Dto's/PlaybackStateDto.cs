namespace SyncService.Domain.Dto_s
{
    public class PlaybackStateDto
    {
        public string RoomId { get; set; }

        public double Position { get; set; }
        public bool IsPaused { get; set; }

        public string VideoId { get; set; }

        public DateTime LastSyncUpdate { get; set; }
    }
}
