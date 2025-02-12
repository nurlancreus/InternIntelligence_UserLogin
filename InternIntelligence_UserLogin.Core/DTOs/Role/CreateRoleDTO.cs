using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.DTOs.Role
{
    public record CreateRoleDTO
    {
        [Required(ErrorMessage = "Role Name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
