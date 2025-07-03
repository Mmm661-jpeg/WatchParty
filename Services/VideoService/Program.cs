using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Driver;
using VideoService.Core.Grpc;
using VideoService.Core.Interfaces;
using VideoService.Core.Services;
using VideoService.Data.DataModels;
using VideoService.Data.ExternalApis.YoutubeApi;
using VideoService.Data.Interfaces;
using VideoService.Data.Repos;
using VideoService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;



var connectionString = config.GetConnectionString("MongoDB")
    ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
var databaseName = config["MongoDB:DatabaseName"]
    ?? throw new InvalidOperationException("MongoDB database name is not configured.");

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(connectionString)
);

builder.Services.AddScoped<IDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return new DbContext(client,databaseName);
});

builder.Services.AddScoped<IVideoRepo,VideoRepo>();
builder.Services.AddScoped<IVideoServices, VideoServices>();

builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();

builder.Services.AddMemoryCache();

builder.Services.AddGrpc();



builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5145, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGrpcService<VideoGrpcServices>();

//app.MapVideoEndpoints();



app.Run();

