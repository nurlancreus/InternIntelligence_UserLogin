using InternIntelligence_UserLogin.API.Endpoints;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Tests.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Tests.Common.Factories
{
    public static partial class Factories
    {
        public static class Role
        {
            public static CreateRoleDTO GenerateAdminRoleRequest()
            {
                return new CreateRoleDTO
                {
                    Name = Constants.Constants.Role.Admin
                };
            }

            public static CreateRoleDTO GenerateSuperAdminRoleRequest()
            {
                return new CreateRoleDTO
                {
                    Name = Constants.Constants.Role.SuperAdmin
                };
            }

            public static IEnumerable<CreateRoleDTO> GenerateAdminRoleRequests()
            {
                yield return new CreateRoleDTO
                {
                    Name = Constants.Constants.Role.SuperAdmin
                };

                yield return new CreateRoleDTO
                {
                    Name = Constants.Constants.Role.Admin
                };
            }

            public static IEnumerable<CreateRoleDTO> GenerateGuestRoleRequests(int guestCount = 4)
            {
                for (int i = 0; i < guestCount; i++)
                {
                    yield return new CreateRoleDTO
                    {
                        Name = $"{Constants.Constants.Role.Guest}-{i}"
                    };
                }
            }
        }
        public static class Auth
        {
            public static string GenerateInValidAccessToken() => Constants.Constants.Auth.InValid_AccessToken;
            public static IEnumerable<RegisterDTO> GenerateValidRegisterRequests(int usersCount = 4)
            {
                for (int i = 1; i <= usersCount; i++)
                {
                    yield return new RegisterDTO
                    {
                        FirstName = $"{Constants.Constants.Auth.FirstName_Valid}-{i}",
                        LastName = $"{Constants.Constants.Auth.LastName_Valid}-{i}",
                        Username = $"{Constants.Constants.Auth.UserName_Valid}-{i}",
                        Email = $"{i}{Constants.Constants.Auth.Email_Valid}",
                        Password = $"{Constants.Constants.Auth.Password_Valid}{i}",
                        ConfirmPassword = $"{Constants.Constants.Auth.Password_Valid}{i}",
                    };
                }
            }
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
