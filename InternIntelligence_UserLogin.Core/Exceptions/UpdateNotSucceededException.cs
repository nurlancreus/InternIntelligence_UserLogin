using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class UpdateNotSucceededException : AppException
    {
        public override string Title => nameof(UpdateNotSucceededException);
        public override string Description => "UpdateNotSucceeded Exception happened.";
        public UpdateNotSucceededException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
        public UpdateNotSucceededException(string message, HttpStatusCode httpStatusCode) : base(message, httpStatusCode)
        {
        }

        public UpdateNotSucceededException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public UpdateNotSucceededException(string message, HttpStatusCode httpStatusCode, Exception inner) : base(message, httpStatusCode, inner) { }

        public UpdateNotSucceededException() : this("Exception happened while updating a entity.") { }
    }
}
