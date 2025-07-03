using System.Security.Claims;
using System.Text.Json;

namespace WatchPartyClient.Utilities
{
    public static class JwtParser
    {
        public static JsonDocument? ParsePayload(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            string payload = PadBase64(parts[1]);

            try
            {
                var jsonBytes = Convert.FromBase64String(payload);
                return JsonDocument.Parse(jsonBytes);
            }
            catch
            {
                return null;
            }
        }

        private static string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                case 1: base64 += "==="; break;
            }
            return base64.Replace('-', '+').Replace('_', '/');
        }

        public static bool IsAnon(string jwt)
        {
            var role = GetRole(jwt);
            return string.IsNullOrWhiteSpace(role) || role.Equals("Anon", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUser(string jwt)
        {
            var role = GetRole(jwt);
            return !string.IsNullOrWhiteSpace(role) && role.Equals("User", StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetUserId(string jwt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jwt))
                    return null;

                var parts = jwt.Split('.');
                if (parts.Length != 3)
                    return null;

                var payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');

                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                using var jsonDoc = JsonDocument.Parse(jsonBytes);

                // Check all possible ID claims in order of priority
                string[] idClaims = new[]
                {
                "UserID",  // Your custom claim
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",  // Standard claim
                "sub"  // JWT standard
            };

                foreach (var claim in idClaims)
                {
                    if (jsonDoc.RootElement.TryGetProperty(claim, out var idProp))
                    {
                        var id = idProp.GetString();
                        Console.WriteLine($"Found UserID in claim '{claim}': {id}");
                        return id;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JWT parsing error: {ex.Message}");
                return null;
            }
        }

        public static bool IsTokenExpired(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');

                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                using var jsonDoc = JsonDocument.Parse(jsonBytes);

                if (jsonDoc.RootElement.TryGetProperty("exp", out var expProp))
                {
                    var exp = expProp.GetInt64();
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    return exp < now;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static string? GetUsername(string jwt)
        {
            var payload = ParsePayload(jwt);
            if (payload == null) return null;

            if (payload.RootElement.TryGetProperty("Username", out var username))
                return username.GetString();

            return null;
        }

        public static string? GetRole(string jwt)
        {
            var payload = ParsePayload(jwt);
            if (payload == null) return null;

            // Check all possible representations of a role claim
            string[] roleClaimKeys = new[]
            {
            "role", "Role",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" // ClaimTypes.Role
        };

            foreach (var key in roleClaimKeys)
            {
                if (payload.RootElement.TryGetProperty(key, out var roleProp))
                    return roleProp.GetString();
            }

            return null;
        }

        public static string? GetAnonUserId(string jwt)
        {
            var payload = ParsePayload(jwt);
            if (payload == null) return null;

            // First, confirm it's actually an Anon token
            var role = GetRole(jwt);
            if (!string.Equals(role, "Anon", StringComparison.OrdinalIgnoreCase))
                return null;

            // Extract AnonUserID
            if (payload.RootElement.TryGetProperty("AnonUserID", out var anonId))
                return anonId.GetString();

            return null;
        }

        public static string? GetRoomId(string jwt)
        {
            var payload = ParsePayload(jwt);
            if (payload == null) return null;

            if (payload.RootElement.TryGetProperty("CurrentRoomId", out var roomIdProp))
                return roomIdProp.GetString();

            return null;
        }

        //public static Task<bool> ValidateRoomToken(string jwt, string roomId, string userId)
        //{
        //    if (string.IsNullOrWhiteSpace(jwt) || string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(userId))
        //        return Task.FromResult(false);

        //    var tokenRoomId = GetRoomId(jwt);
        //    var tokenUserId = GetAnonUserId(jwt);

        //    return Task.FromResult(
        //        !string.IsNullOrEmpty(tokenRoomId) &&
        //        !string.IsNullOrEmpty(tokenUserId) &&
        //        tokenRoomId == roomId &&
        //        tokenUserId == userId
        //    );
        //}
    }

}

 
    
       
