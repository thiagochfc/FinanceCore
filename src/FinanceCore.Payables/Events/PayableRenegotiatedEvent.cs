using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.Events;

public record PayableRenegotiatedEvent(PayableId OriginalPayableId, PayableId NewPayableId) : IDomainEvent;
