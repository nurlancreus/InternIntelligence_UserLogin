using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class PasswordResetException : AppException
    {
        public override string Title => nameof(PasswordResetException);
        public override string Description => "Password Reset Exception happened.";
        public PasswordResetException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public PasswordResetException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public PasswordResetException() : this("Exception happened while reseting password.") { }

    }
}
