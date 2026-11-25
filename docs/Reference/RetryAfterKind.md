# RetryAfterKind Enum
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfterKind.cs" target="_blank">RetryAfterKind.cs</a>

The form in which the source gave a retry hint ([RetryAfter.Kind](RetryAfter/RetryAfter_Kind.md)).

```csharp
public enum RetryAfterKind
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → [ValueType](https://learn.microsoft.com/dotnet/api/system.valuetype) → [Enum](https://learn.microsoft.com/dotnet/api/system.enum) → RetryAfterKind

## Fields
| Name  | Value | Description |
| ----- | ----- | ----------- |
| Delay | 0     | The source gave a number of seconds, e.g., `Retry-After: 120`; [RetryAfter.Delay](RetryAfter/RetryAfter_Delay.md) has a value. |
| Date  | 1     | The source gave a date, e.g., `Retry-After: Wed, 21 Oct 2026 07:28:00 GMT`; [RetryAfter.Delay](RetryAfter/RetryAfter_Delay.md) is `null` and [RetryAfter.RetryAt](RetryAfter/RetryAfter_RetryAt.md) has a value. |

## Remarks
The values are not stored: an error stores the keys of [ResilienceMetadataKeys](ResilienceMetadataKeys.md), and the kind is calculated when the hint is read.

## See Also
* [RetryAfter](RetryAfter/RetryAfter.md)
