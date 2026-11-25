# ErrorResilienceExtensions Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs" target="_blank">ErrorResilienceExtensions.cs</a>

Extension methods of [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md) that store and read how an operation that failed can be retried, and whether it may be.

```csharp
public static class ErrorResilienceExtensions
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → ErrorResilienceExtensions

## Remarks
An `Error` is immutable: the `With…` methods return a changed copy, with the rules of the methods of the same name in [ErrorBuilderResilienceExtensions](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions.md). To set several values at once, change an [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md) instead.

The members are declared in a C# 14 extension block (`extension(Error error)`). They compile to the static extension methods shown on their pages, which every C# version and other .NET languages can call.

## Methods
| Name | Description |
| ---- | ----------- |
| [IsRetryable(Error, TimeProvider)](ErrorResilienceExtensions_IsRetryable.md) | Tells whether the error says that the operation may be retried. |
| [TryGetRetryAfter(Error, RetryAfter, TimeProvider)](ErrorResilienceExtensions_TryGetRetryAfter.md) | Reads the retry hint of the error. |
| [WithRetryable(Error, Boolean)](ErrorResilienceExtensions_WithRetryable.md) | Returns a copy of the error that stores whether the operation may be retried. |
| [WithRetryAfter(Error, DateTimeOffset)](ErrorResilienceExtensions_WithRetryAfter.md#withretryaftererror-datetimeoffset) | Returns a copy of the error that stores the earliest time to retry the operation. |
| [WithRetryAfter(Error, TimeSpan, DateTimeOffset)](ErrorResilienceExtensions_WithRetryAfter.md#withretryaftererror-timespan-datetimeoffset) | Returns a copy of the error that stores a delay that was received, with the time it was received. |
| [WithRetryAfter(Error, TimeSpan)](ErrorResilienceExtensions_WithRetryAfter.md#withretryaftererror-timespan) | Returns a copy of the error that stores the delay before the operation is retried. |

## See Also
* [ErrorBuilderResilienceExtensions](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions.md)
* [OperationResultResilienceExtensions](../OperationResultResilienceExtensions/OperationResultResilienceExtensions.md)
* [RetryAfter](../RetryAfter/RetryAfter.md)
