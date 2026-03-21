using System.Diagnostics.CodeAnalysis;

namespace FinanceCore.Shareds.Errors;

#pragma warning disable CA1032
[SuppressMessage("Design", "CA1062:Validar argumentos de métodos públicos")]
public class DomainException : Exception
#pragma warning restore CA1032
{
    public ContextError ContextError { get; }

    public DomainException(ContextError contextError)
        : base(contextError.Description)
    {
        ContextError = contextError;
    }

    public DomainException(ContextError contextError, Exception innerException)
        : base(contextError.Description, innerException)
    {
        ArgumentNullException.ThrowIfNull(contextError);
        ContextError = contextError;
    }
}
