using UserService.Core.Interfaces;
using UserService.Core.Services;
using UserService.Data.Interfaces;
using UserService.Data.Repository;

namespace UserService.Extensions
{
    public static class AddContainerServices
    {
        public static IServiceCollection AddContainerServicesExtended(this IServiceCollection services)
        {
            services.AddScoped<ITokenGenerator, TokenGenerator>();
            services.AddScoped<IAnonUsersServices, AnonUsersServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IUserRepos, UserRepos>();
          
            return services;
        }
    }
}
