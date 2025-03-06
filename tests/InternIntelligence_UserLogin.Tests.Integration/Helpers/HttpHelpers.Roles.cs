using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Tests.Common.Factories;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public static partial class HttpHelpers
    {
        public static async Task<ICollection<Guid>> CreateRolesAsync(this HttpClient client, string superAdminAccessToken, int rolesCount = 4)
        {
            var roleDtos = Factories.Role.GenerateGuestRoleRequests(rolesCount);
            ICollection<Guid> roleIds = [];

            foreach (var roleDto in roleDtos)
            {
                var roleId = await client.CreateRoleAsync(superAdminAccessToken, roleDto.Name);

                roleIds.Add(roleId);
            }

            return roleIds;
        }

        public static async Task<Guid> CreateRoleAsync(this HttpClient client, string superAdminAccessToken, string roleName = "Guest")
        {
            var roleDto = new CreateRoleDTO { Name = roleName };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/roles")
            {
                Content = JsonContent.Create(roleDto)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superAdminAccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var roleId = await response.Content.ReadFromJsonAsync<Guid>();
            return roleId!;
        }

        public static async Task AssignRoleToUserAsync(this HttpClient client, Guid userId, Guid roleId, string superAdminAccessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/assign-roles")
            {
                Content = JsonContent.Create(new AssignRolesDTO
                {
                    RoleIds = [roleId]
                })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superAdminAccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
