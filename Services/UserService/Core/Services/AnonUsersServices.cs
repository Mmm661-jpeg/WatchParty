using UserService.Core.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.UtilModels;

namespace UserService.Core.Services
{
    public class AnonUsersServices : IAnonUsersServices
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ILogger<AnonUsersServices> _logger;

        public AnonUsersServices(ITokenGenerator tokenGenerator, ILogger<AnonUsersServices> logger)
        {
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<string>> GenerateAnonUserToken(string? currentRoomId, string userName)
        {
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(currentRoomId))
            {
                _logger.LogWarning("UserName or CurrentRoomId is null or empty. Cannot generate token for anonymous user.");
                return null;
            }

            try
            {
                var newAnnonUser = new AnonUsers()
                {
                    UserName = userName,
                    CurrentRoomId = currentRoomId
                };

                var token = await _tokenGenerator.GenerateTokenForAnonUser(newAnnonUser);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token generation failed for anonymous user: {UserName}", userName);
                    return OperationResult<string>.Failure("Failed to generate token",token);
                }
                    _logger.LogInformation("Token generated successfully for anonymous user: {UserName}", userName);
                return OperationResult<string>.Success("Token generated",token);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error generating token for anonymous user.");
                throw;
            }
        }
    }
}
