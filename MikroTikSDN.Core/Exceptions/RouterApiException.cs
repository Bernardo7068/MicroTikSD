using System;
using System.Net;

namespace MikroTikSDN.Core.Exceptions
{
    public class RouterApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? RawBody { get; }

        public RouterApiException(string message, HttpStatusCode statusCode, string? rawBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            RawBody = rawBody;
        }
    }
}