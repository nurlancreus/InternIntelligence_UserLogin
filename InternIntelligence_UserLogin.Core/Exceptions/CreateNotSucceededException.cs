using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class CreateNotSucceededException : AppException
    {
        public override string Title => nameof(CreateNotSucceededException);
        public override string Description => "CreateNotSucceeded Exception happened.";
        public CreateNotSucceededException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
        public CreateNotSucceededException(string message, HttpStatusCode httpStatusCode) : base(message, httpStatusCode)
        {
        }

        public CreateNotSucceededException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public CreateNotSucceededException(string message, HttpStatusCode httpStatusCode, Exception inner) : base(message, httpStatusCode, inner) { }

        public CreateNotSucceededException() : this("Exception happened while creating a new entity.") { }
    }
}
