# RetryAfter.Kind Property
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs#L72" target="_blank">RetryAfter.cs</a>

The form in which the source gave the hint: [RetryAfterKind.Delay](../RetryAfterKind.md) when [Delay](RetryAfter_Delay.md) has a value, otherwise [RetryAfterKind.Date](../RetryAfterKind.md).

```csharp
public RetryAfterKind Kind { get; }
```

### Property Value
[RetryAfterKind](../RetryAfterKind.md)

`Delay` or `Date`.

## Remarks
It is calculated from [Delay](RetryAfter_Delay.md), so it cannot disagree with the values. A hint stored with a delay and the time it was received has both values, and its kind is `Delay`.

## See Also
* [RetryAfter](RetryAfter.md)
* [RetryAfterKind](../RetryAfterKind.md)
