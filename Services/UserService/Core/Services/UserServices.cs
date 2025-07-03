using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Core.Interfaces;
using UserService.Data.Interfaces;
using UserService.Domain.DTO_s;
using UserService.Domain.Entities;
using UserService.Domain.RequestModels;
using UserService.Domain.UtilModels;

namespace UserService.Core.Services
{
    public class UserServices : IUserServices 
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepos _userRepo;
        private readonly ILogger<UserServices> _logger;
        private readonly ITokenGenerator _tokenGenerator;

        public UserServices(UserManager<User> userManager, IUserRepos userRepo, ILogger<UserServices> logger, ITokenGenerator tokenGenerator)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenGenerator = tokenGenerator;
        }

        public async Task<OperationResult<bool>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("No user found with ID: {UserId} to delete.", userId);
                    return OperationResult<bool>.Failure("User not found.");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return OperationResult<bool>.Failure("Failed to delete user");
                }

                return OperationResult<bool>.Success("User deleted successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with ID: {UserId}", userId);
                return OperationResult<bool>.Failure("Unexpected error occurred while deleting user.");
            }
        }

        public async Task<OperationResult<IReadOnlyList<UserDTO>>> GetAllUsersAsync(int page = 1, CancellationToken cancellationToken = default)
        {
            if (page <= 0)
            {
                return OperationResult<IReadOnlyList<UserDTO>>.Failure("Page number must be greater than or equal to 1.");
            }

            try
            {
                var result = await _userRepo.GetAllUsersAsync(page, cancellationToken);

                if (!result.Any())
                {
                    _logger.LogInformation("No users found in the database.");
                    return OperationResult<IReadOnlyList<UserDTO>>.Failure("No users found!");
                }

                var mappedUsers = MapToUserDTOs(result).ToList();
                _logger.LogInformation($"Retrieved {mappedUsers.Count} users from the database on page {page}.");

                return OperationResult<IReadOnlyList<UserDTO>>.Success("Users retrieved successfully.", mappedUsers);

            }
     
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
               return OperationResult<IReadOnlyList<UserDTO>>.Failure("Unexpected error occurred while retrieving users.");
            }
        }

        public async Task<OperationResult<UserDTO>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email must be provided to retrieve user.");
                return OperationResult<UserDTO>.Failure("Email must be provided.");
            }

            try
            {
                var result = await _userRepo.GetUserByEmailAsync(email, cancellationToken);

                if (result == null)
                {
                    _logger.LogInformation("No user found with the provided email: {Email}", email);
                    return OperationResult<UserDTO>.Failure("User not found.");
                }

                var userDto = MapToUserDTO(result);

                _logger.LogInformation("User found with email: {Email}", email);
                return OperationResult<UserDTO>.Success("User retrieved successfully.", userDto);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by email.");
                return OperationResult<UserDTO>.Failure("Unexpected error occurred.");
            }
        }

        public async Task<OperationResult<UserDTO>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("UserId must be provided to retrieve user.");
                return OperationResult<UserDTO>.Failure("UserId must be provided.");
            }

            try
            {
                var result = await _userRepo.GetUserByIdAsync(userId, cancellationToken);

                if (result == null)
                {
                    _logger.LogInformation("No user found with the provided Id: {UserId}", userId);
                    return OperationResult<UserDTO>.Failure("User not found.");
                }

                var userDto = MapToUserDTO(result);

                _logger.LogInformation("User found with Id: {UserId}", userId);
                return OperationResult<UserDTO>.Success("User retrieved successfully.", userDto);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by Id.");
                return OperationResult<UserDTO>.Failure("Unexpected error occurred.");
            }
        }

        public async Task<OperationResult<UserDTO>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Username must be provided to retrieve user.");
                return OperationResult<UserDTO>.Failure("Username must be provided.");
            }

            try
            {
                var result = await _userRepo.GetUserByUsernameAsync(username, cancellationToken);

                if (result == null)
                {
                    _logger.LogInformation("No user found with the provided username: {Username}", username);
                    return OperationResult<UserDTO>.Failure("User not found.");
                }

                var userDto = MapToUserDTO(result);
                _logger.LogInformation("User found with username: {Username}", username);
                return OperationResult<UserDTO>.Success("User retrieved successfully.", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by username.");
                return OperationResult<UserDTO>.Failure("Unexpected error occurred.");
            }
        }

        public async Task<OperationResult<string>> LoginAsync(Login_Req login_Req, CancellationToken cancellationToken = default)
        {
            if(login_Req == null || string.IsNullOrWhiteSpace(login_Req.UserName) || string.IsNullOrWhiteSpace(login_Req.Password))
            {
                _logger.LogWarning("Login attempt failed: Username or password is null or empty.");
                return OperationResult<string>.Failure("Username and password must be provided.");
            }

            try
            {
                var user = await _userManager.FindByNameAsync(login_Req.UserName);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User not found with username {UserName}", login_Req.UserName);
                    return OperationResult<string>.Failure("Invalid username or password.");
                }

                var result = await _userManager.CheckPasswordAsync(user, login_Req.Password);

                if (!result)
                {
                    _logger.LogWarning("Login attempt failed: Incorrect password for user {UserName}", login_Req.UserName);
                    return OperationResult<string>.Failure("Invalid username or password.");
                }

                // Optionally, update last active time or any other logic here
                _logger.LogInformation("User {UserName} logged in successfully.", login_Req.UserName);

                var token = await _tokenGenerator.GenerateTokenForUser(user);


                return OperationResult<string>.Success("Succesfull login",token);

   


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user login.");
                return OperationResult<string>.Failure("An unexpected error occurred during login.");
            }
        }

        public async Task<OperationResult<bool>> RegisterUserAsync(Register_Req req, CancellationToken cancellationToken = default)
        {
            if(req == null || string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.Email))
            {
                _logger.LogWarning("Registration failed: Required fields are missing.");
                return OperationResult<bool>.Failure("Username, password, and email must be provided.");
            }

            try
            {
                var userNameTaken = await _userRepo.UsernameExistsAsync(req.UserName, cancellationToken);

                if (userNameTaken)
                {
                    _logger.LogWarning("Registration failed: Username {UserName} is already taken.", req.UserName);
                    return OperationResult<bool>.Failure("Username is already taken.");
                }

                var normalizedUserName = req.UserName?.Trim();
                var normalizedEmail = req.Email?.Trim();

                var user = new User
                {
                    UserName = normalizedUserName,
                    Email = normalizedEmail,
                    CreatedAt = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, req.Password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Registration failed for user {UserName}: {Errors}", req.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return OperationResult<bool>.Failure("Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                _logger.LogInformation("User {UserName} registered successfully.", req.UserName);

                return OperationResult<bool>.Success("User registered successfully.", true);


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user: {UserName}", req.UserName);
                return OperationResult<bool>.Failure("An unexpected error occurred during registration.");
            }
        }

        public async Task<OperationResult<bool>> UpdateLastActiveAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("UserId must be provided to update last active time.");
                return OperationResult<bool>.Failure("UserId must be provided.");
            }

            try
            {
                var result = await _userRepo.UpdateLastActiveAsync(userId, cancellationToken);

                if (!result)
                {
                    _logger.LogWarning("No user found with ID: {UserId} found or last active time update failed.", userId);
                    return OperationResult<bool>.Failure("User not found or last active time update failed.");
                }

                _logger.LogInformation("Last active time updated successfully for user: {UserId}", userId);
                return OperationResult<bool>.Success("Last active time updated successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating last active time for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<OperationResult<bool>> UpdateUserAsync(Update_Req req, CancellationToken cancellationToken = default)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.UserId))
            {
                _logger.LogWarning("Update failed: UserId must be provided.");
                return OperationResult<bool>.Failure("UserId must be provided.");
            }

            if(string.IsNullOrWhiteSpace(req.UserName) && string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(req.PhoneNumber))
            {
                return OperationResult<bool>.Failure("Update failed: At least one field (Username, Email, PhoneNumber) must be provided for update.");
            }

            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == req.UserId,cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("No user found with ID: {UserId} to update.", req.UserId);
                    return OperationResult<bool>.Failure("User not found.");
                }

                if(!string.IsNullOrWhiteSpace(req.UserName) && !string.Equals(user.UserName, req.UserName,StringComparison.OrdinalIgnoreCase))
                {
                    var userNameTaken = await _userRepo.UsernameExistsAsync(req.UserName, cancellationToken);

                    if (userNameTaken && user.UserName != req.UserName)
                    {
                        _logger.LogWarning("Update failed: Username {Username} is already taken.", req.UserName);
                        return OperationResult<bool>.Failure("Username is already taken.");
                    }
                }

                var newEmail = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email?.Trim();
                var newUserName = string.IsNullOrWhiteSpace(req.UserName) ? null : req.UserName?.Trim();
                var newPhoneNumber = string.IsNullOrWhiteSpace(req.PhoneNumber) ? null : req.PhoneNumber?.Trim();

                user.Email = newEmail ?? user.Email;
                user.UserName = newUserName ?? user.UserName;
                user.LastActive = DateTime.UtcNow;
                user.PhoneNumber = newPhoneNumber ?? user.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Update failed for user {UserId}: {Errors}", req.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return OperationResult<bool>.Failure("Update failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                _logger.LogInformation("User {UserId} updated successfully.", req.UserId);
                return OperationResult<bool>.Success("User updated successfully.", true);



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user: {UserId}", req.UserId);
                throw;
            }
        }

        public async Task<OperationResult<bool>> ChangeUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
            {
                _logger.LogWarning("Invalid userId or newRole provided for changing user role.");
                return OperationResult<bool>.Failure("Invalid userId or newRole provided.");
            }

            if(Enum.TryParse<UserRole>(newRole, true, out var userRole) == false)
            {
                _logger.LogWarning("Invalid role provided: {NewRole}", newRole);
                return OperationResult<bool>.Failure("Invalid role provided.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("No user found with ID: {UserId} to change role.", userId);
                    return OperationResult<bool>.Failure("User not found.");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Contains(userRole.ToString(), StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User {UserId} already has the role {Role}. No changes made.", userId, userRole);
                    return OperationResult<bool>.Success("User already has the specified role.", true);
                }

                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Failed to remove current roles for user {UserId}: {Errors}", userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return OperationResult<bool>.Failure("Failed to remove current roles: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                }

                var addResult = await _userManager.AddToRoleAsync(user, userRole.ToString());

                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add new role {Role} for user {UserId}: {Errors}", userRole, userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return OperationResult<bool>.Failure("Failed to add user to new role");
                }

                user.Role = userRole;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogWarning("Failed to update user record after role change.");
                    return OperationResult<bool>.Failure("Role added but failed to update user record.");
                }

                _logger.LogInformation("User {UserId} role changed to {Role} successfully.", userId, userRole);
                return OperationResult<bool>.Success("User role changed successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing user role for user: {UserId}", userId);
                throw;
            }
        }


        private IEnumerable<UserDTO> MapToUserDTOs(IEnumerable<User> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users), "Users collection cannot be null or empty.");
            }

            try
            {
                return users.Select(MapToUserDTO);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error mapping users to UserDTOs.", ex);
            }


        }

        private UserDTO MapToUserDTO(User user)
        {
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            try
            {
                return new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    LastActive = user.LastActive,
                    CreatedAt = user.CreatedAt,
                    Role = user.Role.ToString(),
                };
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
