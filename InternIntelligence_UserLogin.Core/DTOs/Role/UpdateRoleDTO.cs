using InternIntelligence_UserLogin.Core.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.DTOs.Role
{
    public record UpdateRoleDTO
    {
        [NotEmptyIfNotNull]
        public string? Name { get; set; }
    }
}
