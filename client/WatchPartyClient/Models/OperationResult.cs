using System.Text.Json.Serialization;

namespace WatchPartyClient.Models
{
    public record OperationResult<T>
    {
        [JsonPropertyName("issuccess")]
        public bool IsSuccess { get; init; }

        [JsonPropertyName("data")]
        public T? Data { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }
}
