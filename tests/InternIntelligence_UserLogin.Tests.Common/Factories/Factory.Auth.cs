using InternIntelligence_UserLogin.API.Endpoints;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Tests.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Tests.Common.Factories
{
    public static partial class Factory
    {
        public static class Auth
        {
            public static RegisterDTO GenerateValidRegisterRequest()
            {
                return new RegisterDTO
                {
                    FirstName = Constants.Constants.Auth.FirstName_Valid,
                    LastName = Constants.Constants.Auth.LastName_Valid,
                    Username = Constants.Constants.Auth.UserName_Valid,
                    Email = Constants.Constants.Auth.Email_Valid,
                    Password = Constants.Constants.Auth.Password_Valid,
                    ConfirmPassword = Constants.Constants.Auth.Password_Valid,
                };
            }

            public static LoginDTO GenerateValidLoginRequest()
            {
                return new LoginDTO
                {
                    Email = Constants.Constants.Auth.Email_Valid,
                    Password = Constants.Constants.Auth.Password_Valid,
                };
            }

            public static LoginDTO GenerateInValidLoginRequest()
            {
                return new LoginDTO
                {
                    Email = "wrong-email",
                    Password = "wrong-password",
                };
            }

            public static RegisterDTO GenerateInValidRegisterRequest()
            {
                return new RegisterDTO
                {
                    FirstName = Constants.Constants.Auth.FirstName_InValid,
                    LastName = Constants.Constants.Auth.LastName_InValid,
                    Username = Constants.Constants.Auth.UserName_InValid,
                    Email = Constants.Constants.Auth.Email_InValid,
                    Password = Constants.Constants.Auth.Password_InValid,
                    ConfirmPassword = Constants.Constants.Auth.Password_InValid,
                };
            }

            public static RegisterDTO GeneratePasswordsInValidRegisterRequest()
            {
                return new RegisterDTO
                {
                    FirstName = Constants.Constants.Auth.FirstName_Valid,
                    LastName = Constants.Constants.Auth.LastName_Valid,
                    Username = Constants.Constants.Auth.UserName_Valid,
                    Email = Constants.Constants.Auth.Email_Valid,
                    Password = Constants.Constants.Auth.Password_Valid,
                    ConfirmPassword = "wrong-password",
                };
            }
        }
    }
}
