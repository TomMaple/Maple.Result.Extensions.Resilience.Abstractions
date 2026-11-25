# OperationResultResilienceExtensions.IsRetryableError(IOperationResult, TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/OperationResultResilienceExtensions.cs#L68" target="_blank">OperationResultResilienceExtensions.cs</a>

Tells whether the result failed with an error that may be retried.

```csharp
public static bool IsRetryableError(this IOperationResult result, TimeProvider? timeProvider = null);
```

### Parameters
#### `result` [IOperationResult](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/IOperationResult/IOperationResult.md)
The result: a `Result` or a `Result<T>`.

#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock for a retry hint with a date without an offset, which is readable only while it is in the future; `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system).

### Returns
#### [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)
`false` for a successful result; otherwise, [IsRetryable(Error, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md) of its error.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `result` is `null`.

## Remarks
The answer describes the error. Whether the operation is safe to repeat (idempotent, *RFC 9110*, section 9.2.2) is for the caller to know.

## Examples
```csharp
if (result.IsRetryableError())
{
    // retry, if the operation is safe to repeat
}
```

## See Also
* [ErrorResilienceExtensions.IsRetryable(Error, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md)
* [Is the error retryable?](../../guides/retryable.md)
