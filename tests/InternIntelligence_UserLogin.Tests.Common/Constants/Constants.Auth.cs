﻿using InternIntelligence_UserLogin.API.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Tests.Common.Constants
{
    public static partial class Constants
    {
        public static class Auth
        {
            public const string InValid_AccessToken = "invalid-token";

            public const string FirstName_Valid = "Test";
            public const string LastName_Valid = "Testov";
            public const string UserName_Valid = "TestTestov";
            public const string Email_Valid = "mefobad429@prorsd.com";
            public const string Password_Valid = "Password123!";

            public const string FirstName_InValid = "";
            public const string LastName_InValid = "";
            public const string UserName_InValid = "";
            public const string Email_InValid = "Testexample.com";
            public const string Password_InValid = "pas!";

            public const string SuperAdmin_FirstName = "Super";
            public const string SuperAdmin_LastName = "Adminov";
            public const string SuperAdmin_UserName = "superadmin";
            public const string SuperAdmin_Email = "admin@example.com";
            public const string SuperAdmin_Password= "Ghujtyrtyu456$";
        }

        public static class Role
        {
            public const string Admin = "Admin";
            public const string SuperAdmin = "SuperAdmin";
            public const string Guest = "Guest";

            public const string Invalid = "";
        }
    }
}
