# RetryAfter Record
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs" target="_blank">RetryAfter.cs</a>

The retry hint of an error: how long to wait before the operation is retried, as the source gave it, and when the wait ends.

```csharp
public sealed record RetryAfter
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → RetryAfter<br>
Implements [IEquatable](https://learn.microsoft.com/dotnet/api/system.iequatable-1)&lt;RetryAfter&gt;

## Remarks
An instance comes from [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md) or [OperationResultResilienceExtensions.GetRetryAfter(IOperationResult, TimeProvider)](../OperationResultResilienceExtensions/OperationResultResilienceExtensions_GetRetryAfter.md), and always holds a hint: [Delay](RetryAfter_Delay.md) or [RetryAt](RetryAfter_RetryAt.md) has a value, or both. An error without a hint gives `null`. The record has no public constructor.

Which member to use:

| Member | What it is | Who uses it |
| --- | --- | --- |
| [GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md) | The time that is left: until `RetryAt`, and zero once it has passed; without a `RetryAt`, the `Delay` | a retry pipeline |
| [GetRemainingSeconds(TimeProvider)](RetryAfter_GetRemainingSeconds.md) | The same in whole seconds, rounded up | a package that writes a `Retry-After` header |
| [RetryAt](RetryAfter_RetryAt.md) | When the wait ends (UTC); `null` for a delay whose start is not known | code that must not read the system clock, e.g., an orchestrator (a durable timer) |
| [Delay](RetryAfter_Delay.md) | The delay as the source gave it; `null` for a date | logging, diagnostics |
| [Kind](RetryAfter_Kind.md) | `Delay` or `Date`: which form the source gave | anything that has to tell them apart |

Two instances are equal when their `Delay` and `RetryAt` are equal (`RetryAt` as an instant). `ToString()` is the one that the compiler generates for a record: it writes `Delay`, `Kind` and `RetryAt` in the current culture, for logs and the debugger, not for parsing.

## Properties
| Name | Description |
| ---- | ----------- |
| [Delay](RetryAfter_Delay.md) | The delay as the source gave it (e.g., `Retry-After: 120`), zero or more; `null` if the source gave a date. |
| [Kind](RetryAfter_Kind.md) | The form in which the source gave the hint. |
| [RetryAt](RetryAfter_RetryAt.md) | When the wait ends, in UTC; `null` for a delay whose start is not known. |

## Methods
| Name | Description |
| ---- | ----------- |
| [GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md) | Returns the time that is left until the operation can be retried. |
| [GetRemainingSeconds(TimeProvider)](RetryAfter_GetRemainingSeconds.md) | Returns the time that is left in whole seconds, a fraction rounded up: the value for a `Retry-After` header. |

## Examples
```csharp
if (error.TryGetRetryAfter(out var retryAfter))
{
    await Task.Delay(retryAfter.GetRemaining(timeProvider), timeProvider, cancellationToken);
}
```

## See Also
* [RetryAfterKind](../RetryAfterKind.md)
* [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md)
* [Retry hint](../../guides/retry-hint.md)
