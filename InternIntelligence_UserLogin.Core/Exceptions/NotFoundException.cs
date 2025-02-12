using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class NotFoundException : AppException
    {
        public override string Title => nameof(NotFoundException);
        public override string Description => "NotFound Exception happened.";
        public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }

        public NotFoundException(string message, Exception inner) : base(message, HttpStatusCode.NotFound, inner) { }

        public NotFoundException() : this("Entity is not found") { }

    }
}
