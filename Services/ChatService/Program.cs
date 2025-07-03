using ChatService.Core.Grpc;
using ChatService.Core.Interfaces;
using ChatService.Core.Services;
using ChatService.Data.DataModels;
using ChatService.Data.Interrfaces;
using ChatService.Data.Repos;
using ChatService.Endpoints;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IDbContext>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    var databaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName");
    return new DbContext(connectionString, databaseName);
});

builder.Services.AddScoped<IChatRepo, ChatRepo>();
builder.Services.AddScoped<IChatServices,ChatServices>();

builder.Services.AddGrpc();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5268, listenOptions =>
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

app.MapGrpcService<ChatGrpcService>();

//app.MapChatEndpoints();


app.Run();

