# RetryAfter.RetryAt Property
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs#L84" target="_blank">RetryAfter.cs</a>

When the wait ends, in UTC; `null` for a delay whose start is not known (stored with [ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan)).

```csharp
public DateTimeOffset? RetryAt { get; }
```

### Property Value
[Nullable](https://learn.microsoft.com/dotnet/api/system.nullable-1)&lt;[DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)&gt;

The value of [RetryAfterDate](../ResilienceMetadataKeys.md), in UTC; `null` when that key is absent or not readable.

## Remarks
It may be in the past: the wait is over. It does not depend on a clock, so it suits code that must not read the system clock, e.g., an orchestrator, which is replayed, uses it as the time of a durable timer.

## See Also
* [RetryAfter](RetryAfter.md)
* [GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md)
