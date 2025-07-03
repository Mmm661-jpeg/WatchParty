using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data.DataModel;
using UserService.Domain.Entities;
using UserService.Domain.UtilModels;
using UserService.Endpoints;
using UserService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtModel>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettingsRaw = builder.Configuration.GetSection("JwtSettings");
var issuer = jwtSettingsRaw["Issuer"];
var audience = jwtSettingsRaw["Audience"];
var signingKey = jwtSettingsRaw["Secret"];
if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(signingKey))
{
    throw new ArgumentException("JWT settings are not properly configured in appsettings.json");
}




var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new ArgumentException("Connection string is not configured results in null value");
}

builder.Services.AddDbContext<UserServiceDBContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));



builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<UserServiceDBContext>()
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders();



builder.Services.AddAuthenticationExtension(issuer, audience, signingKey);
builder.Services.AddAuthorization();



//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy =>
//    {
//        policy.RequireAuthenticatedUser();
//        policy.RequireRole("Admin");
//    });
//});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerExtended();

builder.Services.AddContainerServicesExtended();





var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtended();
}






app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapAnnonUserEndpoints();

//app.MapGet("/secure-test", (HttpContext ctx) =>
//{
//    var user = ctx.User;
//    var name = user.Identity?.Name ?? "No name claim";
//    var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "No role claim";

//    return Results.Ok(new { name, role });
//}).RequireAuthorization();



app.Run();

