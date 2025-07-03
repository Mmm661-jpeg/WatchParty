namespace WatchPartyClient.Models
{
    public class PlaybackStateDto
    {
        public double Position { get; set; }
        public bool IsPaused { get; set; }
        public string VideoId { get; set; } = string.Empty;
    }
}
