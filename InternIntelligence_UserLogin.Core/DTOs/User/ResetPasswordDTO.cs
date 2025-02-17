using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.DTOs.User
{
    public record ResetPasswordDTO
    {
        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
