using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using static WatchPartyClient.Pages.Room;
using System.Net.Http.Json;
using WatchPartyClient.Models;
using WatchPartyClient.Utilities;
using System.Net.Http.Headers;

namespace WatchPartyClient.Services
{
    public class RoomServices
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly IConfiguration _config;

        public RoomServices(HttpClient httpClient, IJSRuntime jsRuntime, IConfiguration config)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _config = config;
        }

        public async Task<(RoomDto? room, string userId, bool isOwner, bool joinFailed, string videoId, HubConnection? hubConnection)>
        JoinRoomAsync(string roomId, string token)
        {
            try
            {
                var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");

                // Get room info
                var room = await GetRoomInfoAsync(roomId);
                if (room == null)
                {
                    return (null, null, false, true, null, null);
                }

                // This method should only be called when we have a token
                if (string.IsNullOrEmpty(token))
                {
                    return (room, null, false, true, null, null);
                }

                string userId = JwtParser.GetUserId(token);
                if (userId == null)
                {
                    return (room, null, false, true, null, null);
                }

                // Check if user is owner
                bool isOwner = userId == room.HostId;

                // Join room
                var joinResponse = await _httpClient.PostAsync($"{baseUrl}api/sync/rooms/joinRoom/{roomId}/{userId}", null);
                if (!joinResponse.IsSuccessStatusCode)
                {
                    return (room, userId, isOwner, true, room.CurrentVideoId, null);
                }

                // Create hub connection
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUrl}api/sync/room/hubs/roomSync", options =>
                    {
                        if (!string.IsNullOrEmpty(token))
                            options.AccessTokenProvider = () => Task.FromResult(token);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                await hubConnection.StartAsync();

                return (room, userId, isOwner, false, room.CurrentVideoId, hubConnection);
            }
            catch (Exception)
            {
                return (null, null, false, true, null, null);
            }
        }

        public async Task LeaveRoomAsync(string roomId, string userId)
        {
            try
            {
                var baseUrl = _config["applicationUrl"]!;
                await _httpClient.PutAsync($"{baseUrl}api/sync/rooms/leaveRoom/{roomId}/{userId}", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error leaving room: {ex.Message}");
            }
        }

        //public async Task<List<ChatMessage>> LoadChatMessagesAsync(string roomId)
        //{
        //    try
        //    {
        //        var baseUrl = _config["applicationUrl"]!;
        //        return await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"{baseUrl}api/sync/chat/getMessages?roomId={roomId}") ?? new();
        //    }
        //    catch
        //    {
        //        return new List<ChatMessage> { new() { User = "System", Text = "Failed to load chat." } };
        //    }
        //}

        public async Task<bool> VerifyRoomAccessCodeAsync(string roomId, string accessCode)
        {
            var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");

            var requestData = new { AccessCode = accessCode };
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}api/sync/rooms/validateRoomPass/{roomId}", requestData);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to verify access code for room {roomId}: {response.StatusCode}");
                return false;
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult<bool>>();
                return result?.IsSuccess == true && result.Data == true;
            }
        }

        public async Task<string> RegisterAnonUser(string roomId,string username)
        {
            var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}api/user/annonUsers/generateToken", new
            {
                CurrentRoomId = roomId,
                UserName = username
            });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to register anon user for room {roomId}: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OperationResult<string>>();
            return result?.IsSuccess == true ? result.Data : null;
        }

        public async Task<RoomDto> GetRoomInfoAsync(string roomId)
        {
            var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");
            var response = await _httpClient.GetFromJsonAsync<OperationResult<RoomDto>>($"{baseUrl}api/sync/rooms/getRoomById/{roomId}");
            if (response?.IsSuccess == true && response.Data != null)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Failed to get room info for {roomId}: {response?.Message}");
            }
        }

        public async Task<bool> IsOwner(string hostId, string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return false;

            var userId = JwtParser.GetUserId(jwt);
            if (userId == null) return false;

            return userId == hostId;
        }

        public async Task<bool> UpdateCurrentVideoIdAsync(string roomId, string videoId, string token)
        {
            try
            {
                // Create request with videoId in the body (more secure than query param)
                var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}api/sync/rooms/updateCurrentVideoId/{roomId}")
                {
                    Content = JsonContent.Create(new { videoId }),
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to update video ID. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<OperationResult<bool>>();

                if (result == null)
                {
                    Console.WriteLine("Received null response from server");
                    return false;
                }

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"Server reported error: {result.Message}");
                    return false;
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating video ID: {ex.Message}");
                return false;
            }
        }
        public async Task<OperationResult<bool>> UpdatePlaybackState(string roomId, double position, bool isPaused, string videoId, string token)
        {
            var request = new
            {
                RoomId = roomId,
                Position = position,
                IsPaused = isPaused,
                VideoId = videoId
            };

            var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}api/sync/rooms/updatePlaybackState")
            {
                Content = JsonContent.Create(request)
            };

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(requestMessage);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<bool>>();

            return result ?? new OperationResult<bool> { IsSuccess = false, Message = "Unknown error" };
        }

        public async Task<OperationResult<PlaybackStateDto>> GetPlaybackStateAsync(string roomId)
        {
            var baseUrl = _config["applicationUrl"] ?? throw new InvalidOperationException("Missing app URL");

            var response = await _httpClient.GetAsync($"{baseUrl}api/sync/rooms/getPlaybackState?roomId={roomId}");

            if (!response.IsSuccessStatusCode)
            {
                return new OperationResult<PlaybackStateDto>
                {
                    IsSuccess = false,
                    Message = $"Failed to retrieve playback state: {response.StatusCode}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<OperationResult<PlaybackStateDto>>();

            return result ?? new OperationResult<PlaybackStateDto>
            {
                IsSuccess = false,
                Message = "Unexpected null response from server"
            };
        }
    }

}
