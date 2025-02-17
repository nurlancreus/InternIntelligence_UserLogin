using System.Text.Json.Serialization;

namespace InternIntelligence_UserLogin.Core.DTOs.User
{
    public record AssignRolesDTO
    {
        [JsonPropertyName("RoleIds")]
        public IEnumerable<Guid> RoleIds { get; set; } = [];
    }
}
