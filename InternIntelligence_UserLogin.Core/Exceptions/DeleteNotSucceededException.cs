using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class DeleteNotSucceededException : AppException
    {
        public override string Title => nameof(DeleteNotSucceededException);
        public override string Description => "DeleteNotSucceeded Exception happened.";
        public DeleteNotSucceededException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
        public DeleteNotSucceededException(string message, HttpStatusCode httpStatusCode) : base(message, httpStatusCode)
        {
        }

        public DeleteNotSucceededException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public DeleteNotSucceededException(string message, HttpStatusCode httpStatusCode, Exception inner) : base(message, httpStatusCode, inner) { }

        public DeleteNotSucceededException() : this("Exception happened while deleting a entity.") { }
    }
}
