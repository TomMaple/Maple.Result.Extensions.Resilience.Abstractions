# ErrorResilienceExtensions.IsRetryable(Error, TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L73" target="_blank">ErrorResilienceExtensions.cs</a>

Tells whether the error says that the operation may be retried.

```csharp
public static bool IsRetryable(this Error error, TimeProvider? timeProvider = null);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock for a retry hint with a date without an offset, which is readable only while it is in the future (passed on to [TryGetRetryAfter(Error, RetryAfter, TimeProvider)](ErrorResilienceExtensions_TryGetRetryAfter.md)); `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system).

### Returns
#### [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)
`true` if the error may be retried; otherwise, `false`.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.

## Remarks
The first rule that matches gives the answer:

| # | Rule | Result |
| --- | --- | --- |
| 1 | **The flag.** The value of [Retryable](../ResilienceMetadataKeys.md) is a `Boolean` | its value: `true` makes any error retryable; `false` forbids a retry, also with a retry hint |
| 2 | **The hint.** The retry hint is readable ([TryGetRetryAfter(Error, RetryAfter, TimeProvider)](ErrorResilienceExtensions_TryGetRetryAfter.md)) | `true`, whatever the category (e.g., a `Validation` error of a `413` response with `Retry-After`) |
| 3 | **The category.** `Unavailable`, `Timeout` or `LimitExceeded` | `true` |
| 4 | Otherwise | `false`, also for `Critical` and `NotImplemented` |

The key of the flag is found whatever its case (e.g., `Retryable` taken from a response body). A value that is not a `Boolean` (e.g., the text `"true"`) counts as absent, and the next rule applies.

> [!IMPORTANT]
> The answer describes the **error**. Whether the operation is safe to repeat (idempotent, *RFC 9110*, section 9.2.2) is for the caller to know: e.g., a payment that timed out may have been made.

## Examples
```csharp
if (error.IsRetryable() && request.IsIdempotent)
{
    // retry
}
```

## See Also
* [WithRetryable(Error, Boolean)](ErrorResilienceExtensions_WithRetryable.md)
* [OperationResultResilienceExtensions.IsRetryableError(IOperationResult, TimeProvider)](../OperationResultResilienceExtensions/OperationResultResilienceExtensions_IsRetryableError.md)
* [Is the error retryable?](../../guides/retryable.md)
* [RFC 9110, section 9.2.2: Idempotent Methods](https://www.rfc-editor.org/rfc/rfc9110#section-9.2.2)
