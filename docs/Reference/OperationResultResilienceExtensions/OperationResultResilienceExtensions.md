# OperationResultResilienceExtensions Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/OperationResultResilienceExtensions.cs" target="_blank">OperationResultResilienceExtensions.cs</a>

Extension methods of [IOperationResult](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/IOperationResult/IOperationResult.md) (`Result` and `Result<T>`) that read how a failed operation can be retried.

```csharp
public static class OperationResultResilienceExtensions
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → OperationResultResilienceExtensions

## Remarks
They read the error of a failed result as the methods of [ErrorResilienceExtensions](../ErrorResilienceExtensions/ErrorResilienceExtensions.md) do; a successful result has no hint and is not retryable.

The members are declared in a C# 14 extension block (`extension(IOperationResult result)`). They compile to the static extension methods shown on their pages, which every C# version and other .NET languages can call.

## Methods
| Name | Description |
| ---- | ----------- |
| [GetRetryAfter(IOperationResult, TimeProvider)](OperationResultResilienceExtensions_GetRetryAfter.md) | Returns the retry hint of a failed result. |
| [IsRetryableError(IOperationResult, TimeProvider)](OperationResultResilienceExtensions_IsRetryableError.md) | Tells whether the result failed with an error that may be retried. |

## See Also
* [ErrorResilienceExtensions](../ErrorResilienceExtensions/ErrorResilienceExtensions.md)
* [RetryAfter](../RetryAfter/RetryAfter.md)
