﻿using System.Net;

namespace InternIntelligence_UserLogin.Core.Exceptions
{
    public class AppException : Exception
    {
        public virtual string Title => nameof(AppException);
        public virtual string Description => "Application Exception happened.";
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public AppException(string message, HttpStatusCode httpStatusCode) : base(message)
        {
            StatusCode = httpStatusCode;
        }
        public AppException(string message, HttpStatusCode httpStatusCode, Exception inner) : base(message, inner)
        {
            StatusCode = httpStatusCode;
        }

    }
}
