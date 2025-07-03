using System.Text.Json.Serialization;

namespace VideoService.Data.ExternalApis.YoutubeApi.YoutubeDto_s
{
    public class YouTubeApiSearchResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeSearchItem>? Items { get; set; }
    }

    public class YouTubeSearchItem
    {
        [JsonPropertyName("id")]
        public SearchId? Id { get; set; }

        [JsonPropertyName("snippet")]
        public Snippet? Snippet { get; set; }
    }

    public class SearchId
    {
        [JsonPropertyName("videoId")]
        public string? VideoId { get; set; }
    }

    public class YouTubeSearchResult
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ChannelName { get; set; }
    }
}
