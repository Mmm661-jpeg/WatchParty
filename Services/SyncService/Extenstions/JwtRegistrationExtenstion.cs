using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace SyncService.Extenstions
{
    public static class JwtRegistrationExtenstion
    {
        public static IServiceCollection AddAuthenticationExtended(this IServiceCollection services, string issuer, string audience, string signinkey)
        {
            if (string.IsNullOrEmpty(issuer))
            {
                throw new ArgumentException("Issuer cannot be null or empty", nameof(issuer));
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new ArgumentException("Audience cannot be null or empty", nameof(audience));
            }

            if (string.IsNullOrEmpty(signinkey))
            {
                throw new ArgumentException("Signing key cannot be null or empty", nameof(signinkey));
            }

            // Add JWT authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signinkey)),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier


                    };
                });

            return services;
        }
    }
}
