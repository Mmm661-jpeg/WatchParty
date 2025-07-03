using Microsoft.AspNetCore.Server.Kestrel.Core;
using RoomService.Core.Grpc;
using RoomService.Core.Interfaces;
using RoomService.Core.Services;
using RoomService.Data.DataModels;
using RoomService.Data.Interfaces;
using RoomService.Data.Repos;
using RoomService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRoomsRepo, RoomsRepo>();
builder.Services.AddScoped<IRoomsServices, RoomsServices>();

builder.Services.AddSingleton<IDbContext>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    var databaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName");
    return new DbContext(connectionString, databaseName);
});

builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5087, listenOptions =>
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

app.MapGrpcService<RoomGrpcService>();

//app.MapRoomEndpoints();





app.Run();

