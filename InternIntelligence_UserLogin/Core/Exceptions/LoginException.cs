using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class LoginException : AppException
    {
        public override string Title => nameof(LoginException);
        public override string Description => "Login Exception happened.";
        public LoginException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public LoginException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public LoginException() : this("Wrong credentials.") { }

    }
}
