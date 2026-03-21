namespace FinanceCore.Shareds.Errors;

public record ContextError(string Code, string Description)
{
    public static ContextError Failure(string code, string description)
        => new(code, description);

    public static ContextError Validation(string code, string description)
        => new(code, description);
}
