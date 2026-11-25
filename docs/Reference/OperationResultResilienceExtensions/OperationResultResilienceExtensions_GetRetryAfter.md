# OperationResultResilienceExtensions.GetRetryAfter(IOperationResult, TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/OperationResultResilienceExtensions.cs#L42" target="_blank">OperationResultResilienceExtensions.cs</a>

Returns the retry hint of a failed result.

```csharp
public static RetryAfter? GetRetryAfter(this IOperationResult result, TimeProvider? timeProvider = null);
```

### Parameters
#### `result` [IOperationResult](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/IOperationResult/IOperationResult.md)
The result: a `Result` or a `Result<T>`.

#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock for a date without an offset, which is readable only while it is in the future ([TimeProvider.GetUtcNow](https://learn.microsoft.com/dotnet/api/system.timeprovider.getutcnow)); `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system).

### Returns
#### [RetryAfter](../RetryAfter/RetryAfter.md)
The hint; `null` for a successful result, and for a failed one without a readable hint. The time that is left is [GetRemaining(TimeProvider)](../RetryAfter/RetryAfter_GetRemaining.md).

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `result` is `null`.

## Remarks
The hint is read as [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md) reads it.

## Examples
```csharp
Result<Order> result = await client.GetOrderAsync(id, cancellationToken);

if (result.GetRetryAfter() is { } retryAfter)
{
    logger.LogInformation("Retry in {Seconds} s.", retryAfter.GetRemainingSeconds());
}
```

## See Also
* [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md)
* [RetryAfter](../RetryAfter/RetryAfter.md)
* [Retry hint](../../guides/retry-hint.md)
