using Microsoft.AspNetCore.Server.Kestrel.Core;
using SharedProtos;
using SyncService.Core.Interfaces;
using SyncService.Core.Services;
using SyncService.Domain.UtilModels;
using SyncService.Endpoints;
using SyncService.Extenstions;
using SyncService.Hubs;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);


builder.Services.Configure<JwtModel>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettingsRaw = builder.Configuration.GetSection("JwtSettings");
var issuer = jwtSettingsRaw["Issuer"];
var audience = jwtSettingsRaw["Audience"];
var signingKey = jwtSettingsRaw["Secret"];
if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(signingKey))
{
    throw new ArgumentException("JWT settings are not properly configured in appsettings.json");
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExtended();

builder.Services.AddScoped<IVideoSyncService,VideoSyncService>();
builder.Services.AddScoped<IChatSyncService,ChatSyncService>();
builder.Services.AddScoped<IRoomSyncService, RoomSyncService>();



builder.Services.AddGrpcClient<ChatService.ChatServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:ChatServiceUrl"]);
});

builder.Services.AddGrpcClient<RoomService.RoomServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:RoomServiceUrl"]);
});

builder.Services.AddGrpcClient<VideoService.VideoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:VideoServiceUrl"]);
});




builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("basic", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddAuthenticationExtended(issuer,audience,signingKey);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NotAnon", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var isAnon = context.User.IsInRole("Anon");
            return !isAnon; 
        });
    });
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options => {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwaggerExtended();


//app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();    
app.UseAuthorization();


app.MapRoomSyncEndpoints();
app.MapChatSyncEndpoints();
app.MapVideoSyncEndpoints();

app.MapHub<RoomSyncHub>("/api/sync/room/hubs/roomSync"); 

app.Run();

