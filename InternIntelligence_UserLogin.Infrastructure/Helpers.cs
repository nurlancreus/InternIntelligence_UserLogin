using InternIntelligence_UserLogin.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Infrastructure
{
    public static class Helpers
    {
        public static string GetIdentityResultError(IdentityResult result)
        {
            var errors = result.Errors.Select(e => e.Description);
            return string.Join(", ", errors);
        }
    }
}
