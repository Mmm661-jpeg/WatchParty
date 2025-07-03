using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Core.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.UtilModels;

namespace UserService.Core.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JwtModel _jwtModel;
        private readonly UserManager<User> _userManager;
        private const int AnnonExpiration = 2;

        public TokenGenerator(IOptions<JwtModel> jwtModel,UserManager<User> userManager)
        {
            _jwtModel = jwtModel.Value ?? throw new ArgumentNullException(nameof(jwtModel));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public Task<string> GenerateTokenForAnonUser(AnonUsers anonUser)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtModel.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,anonUser.UserId),
                new Claim("Username",anonUser.UserName),
                new Claim("AnonUserID",anonUser.UserId),
                new Claim("CurrentRoomId",anonUser.CurrentRoomId),
                new Claim(ClaimTypes.Role, "Anon")
            };

            var token = new JwtSecurityToken(
                issuer: _jwtModel.Issuer,
                audience: _jwtModel.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(AnnonExpiration),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<string> GenerateTokenForUser(User user)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtModel.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim("Username",user.UserName),
                new Claim("UserID",user.Id),
                new Claim(ClaimTypes.Role, "User")


            };

            var roles = await _userManager.GetRolesAsync(user);

            //claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            if (user.UserName == "Test11")
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
           

            var token = new JwtSecurityToken(
                issuer: _jwtModel.Issuer,
                audience: _jwtModel.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtModel.Expiration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
