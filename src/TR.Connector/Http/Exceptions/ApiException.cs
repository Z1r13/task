namespace TR.Connector.Http.Exceptions;

/// <summary>
/// Исключение при ошибках Api (success = false)
/// </summary>
internal class ApiException : Exception
{
    public ApiException(string message)
        : base(message) { }

    public ApiException(string message, Exception innerException)
        : base(message, innerException) { }
}
