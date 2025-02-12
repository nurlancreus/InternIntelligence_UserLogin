using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class ValidationException : AppException
    {
        public override string Title => nameof(ValidationException);
        public override string Description => "Validation Exception happened.";

        public ICollection<string> Errors = [];
        public ValidationException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public ValidationException(ICollection<string> errors) : base(string.Join(", ", errors), HttpStatusCode.BadRequest)
        {
            Errors = errors;
        }

        public ValidationException(string message, Exception inner) : base(message, HttpStatusCode.BadRequest, inner) { }

        public ValidationException() : this("Validation error happened.") { }

    }
}
