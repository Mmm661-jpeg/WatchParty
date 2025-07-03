using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using WatchPartyClient;
using WatchPartyClient.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(sp => httpClient);

// Download and load appsettings.json from wwwroot
var configStream = await httpClient.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(configStream);


//var baseSignalRUrl = builder.Configuration.GetValue<string>("applicationUrl") ?? throw  new ArgumentNullException("SignalR:BaseUrl is not configured in appsettings.json");


//var hubConnection = new HubConnectionBuilder()
//    .WithUrl($"{baseSignalRUrl}api/sync/hubs/roomSync", options =>
//    {
//        options.AccessTokenProvider = async () =>
//        {
//            var token = await localStorage.GetItemAsync<string>("authToken");
//            return token;
//        };
//    })
//    .WithAutomaticReconnect()
//    .Build();


builder.Services.AddScoped<RoomServices>();


await builder.Build().RunAsync();
