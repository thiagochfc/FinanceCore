using System.Text.RegularExpressions;

using FinanceCore.Shareds.Errors;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Common.ValueObjects;

public sealed partial class DocumentNumber : ValueObject
{
    [GeneratedRegex(@"\p{Cc}", RegexOptions.Compiled)]
    private static partial Regex ControlCharsRegex();

    private const int MaximumLength = 50;

    public string Value { get; }

    private DocumentNumber(string value) =>
        Value = value;

    public static DocumentNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var sanitized = ControlCharsRegex().Replace(value, "");

        // Validating again, as it may be empty after cleaning.
        ArgumentException.ThrowIfNullOrWhiteSpace(sanitized);
        ThrowIfLengthIsTooLong(sanitized);

        return new DocumentNumber(sanitized);
    }

    private static void ThrowIfLengthIsTooLong(string value)
    {
        if (value.Length <= MaximumLength)
        {
            return;
        }

        ContextError contextError = new("DocumentNumber.TooLong",
            $"DocumentNumber is too long. Maximum allowed length is: {MaximumLength}");
        throw new TooLongDocumentNumberException(contextError);
    }

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}

#pragma warning disable CA1032
public sealed class TooLongDocumentNumberException(ContextError contextError) : DomainException(contextError);
#pragma warning restore CA1032
