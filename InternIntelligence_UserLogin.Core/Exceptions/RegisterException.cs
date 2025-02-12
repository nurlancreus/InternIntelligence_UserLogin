using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class RegisterException : AppException
    {
        public override string Title => nameof(RegisterException);
        public override string Description => "Register Exception happened.";
        public RegisterException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public RegisterException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public RegisterException() : this("Exception happened while registered.") { }
        
    }
}
