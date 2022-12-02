namespace Borealis.Shared.Extensions;


public class ExceptionInfo
{
    public string Type { get; init; }

    public string Message { get; init; }

    public string? Source { get; init; }

    public string? StackTrace { get; init; }

    public ExceptionInfo? InnerException { get; init; }


    public ExceptionInfo() { }


    public ExceptionInfo(Exception exception, bool includeInnerException = true, bool includeStackTrace = false)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        Type = exception.GetType().FullName!;
        Message = exception.Message;
        Source = exception.Source;
        StackTrace = includeStackTrace ? exception.StackTrace : null;

        if (includeInnerException && exception.InnerException is not null)
        {
            InnerException = new ExceptionInfo(exception.InnerException, includeInnerException, includeStackTrace);
        }
    }
}