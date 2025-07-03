using System.Text.Json.Serialization;

namespace WatchPartyClient.Models
{
    public class RoomDto
    {
        [JsonPropertyName("roomId")]
        public string RoomId { get; set; }

        [JsonPropertyName("roomName")]
        public string RoomName { get; set; }

        [JsonPropertyName("maxOccupancy")]
        public int MaxOccupancy { get; set; }

        [JsonPropertyName("hostId")]
        public string HostId { get; set; }

        [JsonPropertyName("participantIds")]
        public List<string> ParticipantIds { get; set; }

        [JsonPropertyName("currentVideoId")]
        public string? CurrentVideoId { get; set; }

        [JsonPropertyName("currentPlaybackPosition")]
        public double CurrentPlaybackPosition { get; set; }

        [JsonPropertyName("isPaused")]
        public bool IsPaused { get; set; }

        [JsonPropertyName("lastSyncUpdate")]
        public DateTime LastSyncUpdate { get; set; }

        [JsonPropertyName("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
