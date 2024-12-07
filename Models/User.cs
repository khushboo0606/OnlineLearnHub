using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace OnlineLearnHub.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [JsonPropertyName("userName")]
        public override string? UserName { get; set; } // Keep it nullable
    }
}
