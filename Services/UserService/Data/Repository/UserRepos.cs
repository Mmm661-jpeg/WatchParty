using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using UserService.Data.DataModel;
using UserService.Data.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Data.Repository
{
    public class UserRepos:IUserRepos
    {
        private readonly UserServiceDBContext _context;
        private readonly ILogger<UserRepos> _logger;
        private const int pageSize = 100;

        public UserRepos(UserServiceDBContext context, ILogger<UserRepos> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(int page = 1,CancellationToken cancellationToken = default)
        {
          
            try
            {
                var result = await _context.Users
                                    .AsNoTracking()
                                    .OrderBy(u => u.Id) 
                                   .Skip((page -1) * pageSize)
                                   .Take(pageSize).ToListAsync(cancellationToken);
                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users.");
                throw;
            }
        }

        public async  Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
  

            try
            {
                var result = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email,cancellationToken);

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId,CancellationToken cancellationToken = default)
        {

            try
            {
                var result = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId,cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username,CancellationToken cancellationToken = default)
        {
           
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == username,cancellationToken);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                throw;
            }
        }

        public async Task<bool> UpdateLastActiveAsync(string userId, CancellationToken cancellationToken = default)
        {

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return false;
                }

                user.LastActive = DateTime.UtcNow;

                _context.Users.Update(user);
                var result = await _context.SaveChangesAsync(cancellationToken);

                if (result > 0)
                {
                    _logger.LogInformation("Updated last active time for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update last active time for user: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last active time for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username,CancellationToken cancellationToken = default)
        {
          

            try
            {
                var normalizedUserName = username.Trim().ToLowerInvariant();

                return await _context.Users
                    .AnyAsync(u => u.UserName == normalizedUserName,cancellationToken);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists with ID: {Username-}", username);
                throw;
            }
        }
    }
}
