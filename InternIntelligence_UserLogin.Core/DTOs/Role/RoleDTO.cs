using InternIntelligence_UserLogin.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.DTOs.Role
{
    public record RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public RoleDTO(ApplicationRole role)
        {
            Id = role.Id;
            Name = role.Name!;
            CreatedAt = role.CreatedAt;
            UpdatedAt = role.UpdatedAt;
        }
    }
}
