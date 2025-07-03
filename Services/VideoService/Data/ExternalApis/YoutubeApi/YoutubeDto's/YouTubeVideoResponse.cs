using System.Text.Json.Serialization;

namespace VideoService.Data.ExternalApis.YoutubeApi.YoutubeDto_s
{
    public class YouTubeApiVideoResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeVideoItem>? Items { get; set; }
    }

    public class YouTubeVideoItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("snippet")]
        public Snippet? Snippet { get; set; }

        [JsonPropertyName("contentDetails")]
        public ContentDetails? ContentDetails { get; set; }
    }

    public class Snippet
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("thumbnails")]
        public Thumbnails? Thumbnails { get; set; }

        [JsonPropertyName("channelTitle")]
        public string? ChannelTitle { get; set; }
    }

    public class Thumbnails
    {
        [JsonPropertyName("default")]
        public Thumbnail? Default { get; set; }

        [JsonPropertyName("medium")]
        public Thumbnail? Medium { get; set; }

        [JsonPropertyName("high")]
        public Thumbnail? High { get; set; }
    }

    public class Thumbnail
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class ContentDetails
    {
        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        public static double ParseYouTubeDuration(string duration)
        {
            if(string.IsNullOrWhiteSpace(duration))
            {
                return 0;
            }

            try
            {
                var timeSpan = System.Xml.XmlConvert.ToTimeSpan(duration);
                return timeSpan.TotalSeconds;
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid YouTube duration format: {duration}", ex);
            }
        }

        
    }

   
}
