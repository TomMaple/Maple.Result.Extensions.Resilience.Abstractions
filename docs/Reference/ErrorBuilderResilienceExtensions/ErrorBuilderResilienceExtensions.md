# ErrorBuilderResilienceExtensions Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorBuilderResilienceExtensions.cs" target="_blank">ErrorBuilderResilienceExtensions.cs</a>

Extension methods of [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md) that store how an operation that failed can be retried, and whether it may be.

```csharp
public static class ErrorBuilderResilienceExtensions
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → ErrorBuilderResilienceExtensions

## Remarks
Which overload of `WithRetryAfter` to use depends on what the code knows:

| Written with | When | [RetryAfterSeconds](../ResilienceMetadataKeys.md) | [RetryAfterDate](../ResilienceMetadataKeys.md) |
| --- | --- | --- | --- |
| [WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan-datetimeoffset) | a delay was **received** (e.g., in a response) | the delay | `receivedAt` + the delay |
| [WithRetryAfter(ErrorBuilder, DateTimeOffset)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-datetimeoffset) | a date | absent | the date |
| [WithRetryAfter(ErrorBuilder, TimeSpan)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan) | a delay whose start is not known, e.g., an error that is created once and reused | the delay | absent |

The members are declared in a C# 14 extension block (`extension(ErrorBuilder builder)`). They compile to the static extension methods shown on their pages, which every C# version and other .NET languages can call.

## Methods
| Name | Description |
| ---- | ----------- |
| [WithRetryable(ErrorBuilder, Boolean)](ErrorBuilderResilienceExtensions_WithRetryable.md) | Stores under [Retryable](../ResilienceMetadataKeys.md) whether the operation may be retried, whatever the category and the retry hint say. |
| [WithRetryAfter(ErrorBuilder, DateTimeOffset)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-datetimeoffset) | Stores the earliest time to retry the operation, and removes the delay. |
| [WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan-datetimeoffset) | Stores a delay that was received, with the time it was received: the delay, and when the wait ends. |
| [WithRetryAfter(ErrorBuilder, TimeSpan)](ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan) | Stores the delay before the operation is retried, in whole seconds, and removes the date. |

## See Also
* [ErrorResilienceExtensions](../ErrorResilienceExtensions/ErrorResilienceExtensions.md), the same members on an `Error`
* [ResilienceMetadataKeys](../ResilienceMetadataKeys.md)
* [Retry hint](../../guides/retry-hint.md)
