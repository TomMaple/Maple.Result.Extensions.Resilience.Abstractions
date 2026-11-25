# RetryAfter.Delay Property
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs#L63" target="_blank">RetryAfter.cs</a>

The delay as the source gave it (e.g., `Retry-After: 120`), zero or more; `null` if the source gave a date.

```csharp
public TimeSpan? Delay { get; }
```

### Property Value
[Nullable](https://learn.microsoft.com/dotnet/api/system.nullable-1)&lt;[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)&gt;

The value of [RetryAfterSeconds](../ResilienceMetadataKeys.md), in whole seconds; `null` when that key is absent or not readable.

## Remarks
It does not get shorter while time passes: use [GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md) for the time that is left. When [RetryAt](RetryAfter_RetryAt.md) has a value too, the delay is what the source gave, and `RetryAt` says when the wait ends.

## See Also
* [RetryAfter](RetryAfter.md)
* [Kind](RetryAfter_Kind.md)
