using System.Runtime.Serialization;

namespace FacadeAccountCreation.Core.Exceptions;

[Serializable]
public class ProblemResponseException : Exception
{
    public ProblemDetails ProblemDetails { get; }
    
    public HttpStatusCode StatusCode { get; }
    
    public ProblemResponseException(ProblemDetails problemDetailsDetails, HttpStatusCode statusCode)
        : base($"Problem response received: StatusCode = {(int)statusCode}, Type = {problemDetailsDetails?.Type}, Detail = {problemDetailsDetails?.Detail}")
    {
        ProblemDetails = problemDetailsDetails;
        StatusCode = statusCode;
    }

    public ProblemResponseException()
    {
    }

    public ProblemResponseException(string message) 
        : base(message)
    {
    }

    public ProblemResponseException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051
    protected ProblemResponseException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
#pragma warning restore SYSLIB0051
}