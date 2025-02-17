using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.DTOs.Role
{
    public record AssignUsersDTO
    {
        public IEnumerable<string> UserNames { get; set; } = [];
    }
}
