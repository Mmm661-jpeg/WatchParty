namespace UserService.Domain.DTO_s
{
    public class UserDTO
    {
        public string Id { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime LastActive { get; init; }

        public string Role { get; init; }

        // Parameterless constructor for serialization/deserialization
        public UserDTO() { }

        public UserDTO(string id, string username, string email, DateTime createdAt, DateTime lastActive, string role)
        {
            Id = id;
            Username = username;
            Email = email;
            CreatedAt = createdAt;
            LastActive = lastActive;
            Role = role;
        }

    }
}
