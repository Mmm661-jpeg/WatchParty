using VideoService.Data.ExternalApis.YoutubeApi.YoutubeDto_s;
using VideoService.Domain.Entities;


namespace VideoService.Data.ExternalApis.YoutubeApi
{
    public class YouTubeService:IYouTubeService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://www.googleapis.com/youtube/v3/";
        private readonly ILogger<YouTubeService> _logger;
        private readonly IConfiguration _configuration;

        public YouTubeService(HttpClient httpClient,ILogger<YouTubeService> logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Video> GetVideoMetadataAsync(string videoId)
        {
            if(string.IsNullOrWhiteSpace(videoId))
            {
                throw new ArgumentException("Video ID cannot be null or empty.", nameof(videoId));
            }
            try
            {
                var normalizedVideoId = videoId.Trim();
                var firstPart = $"videos?part=snippet,contentDetails&id={normalizedVideoId}&key=";
                //var secondPart = "&key=AIzaSyCuvIMFBEhkqB42dq974xQlpYIumQ_xP44"; // Replace with your actual API key
                var secondPart = _configuration["YouTubeApiSecondPart"];


                 string Url = BaseUrl + firstPart + secondPart;

                var response = await _httpClient.GetAsync(Url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve video metadata for video ID: {VideoId}. Status code: {StatusCode}", videoId, response.StatusCode);
                    throw new Exception($"Failed to retrieve video metadata. Status code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var videoResponse = System.Text.Json.JsonSerializer.Deserialize<YouTubeApiVideoResponse>(content);

                if (videoResponse == null || videoResponse.Items == null || videoResponse.Items.Count == 0)
                {
                    _logger.LogWarning("No video metadata found for video ID: {VideoId}", videoId);
                    throw new Exception($"No video metadata found for video ID: {videoId}");
                }

                var item = videoResponse.Items[0];
                var contentDetails = item.ContentDetails;
                var parsedDuration = contentDetails?.Duration != null ? ContentDetails.ParseYouTubeDuration(contentDetails.Duration) : 0;
                var video = new Video
                {
                    
                    VideoId = item.Id,
                    Title = item.Snippet?.Title,
                    Platform = Platform.YouTube,
                    ThumbnailUrl = item.Snippet?.Thumbnails?.Default?.Url ?? item.Snippet?.Thumbnails?.Medium?.Url ?? item.Snippet?.Thumbnails?.High?.Url,
                    Duration =parsedDuration,
                    ChannelName = item.Snippet?.ChannelTitle,
                    AddedAt = DateTime.UtcNow,

                };

                _logger.LogInformation("Successfully retrieved video metadata for video ID: {VideoId}", videoId);
                return video;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video metadata for video ID: {VideoId}", videoId);
                throw new Exception("Error retrieving video metadata", ex);
            }
        }

        public async Task<List<YouTubeSearchResult>> SearchVideosAsync(string query, int maxResults = 10)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query cannot be null or empty.", nameof(query));
            }

            try
            {
                var normalizedQuery = query.Trim();
                var firstPart = $"search?part=snippet&type=video&maxResults={maxResults}&q={Uri.EscapeDataString(normalizedQuery)}&key=";
                var secondPart = _configuration["YouTubeApiSecondPart"]; 

                var fullUrl = BaseUrl + firstPart + secondPart;

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to search videos with query: {Query}. Status code: {StatusCode}", query, response.StatusCode);
                    throw new Exception($"Failed to search videos. Status code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();

                var searchResponse = System.Text.Json.JsonSerializer.Deserialize<YouTubeApiSearchResponse>(content);

                if (searchResponse == null || searchResponse.Items == null || searchResponse.Items.Count == 0)
                {
                    _logger.LogWarning("No videos found for query: {Query}", query);
                    return null;
                }

              

                var results = searchResponse.Items
                             .Where(i => i.Id?.VideoId != null)
                             .Select(item => new YouTubeSearchResult
                             {
                                 VideoId = item.Id!.VideoId!,
                                 Title = item.Snippet?.Title,
                                 ThumbnailUrl = item.Snippet?.Thumbnails?.Default?.Url
                                                 ?? item.Snippet?.Thumbnails?.Medium?.Url
                                                 ?? item.Snippet?.Thumbnails?.High?.Url,
                                 ChannelName = item.Snippet?.ChannelTitle
                             }).ToList();

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching videos with query: {Query}", query);
                throw new Exception("Error searching videos", ex);
            }
        }
    }
}
