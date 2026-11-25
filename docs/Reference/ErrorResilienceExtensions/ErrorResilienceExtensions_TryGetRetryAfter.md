# ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L133" target="_blank">ErrorResilienceExtensions.cs</a>

Reads the retry hint of the error.

```csharp
public static bool TryGetRetryAfter(this Error error, out RetryAfter? retryAfter, TimeProvider? timeProvider = null);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `retryAfter` [RetryAfter](../RetryAfter/RetryAfter.md)
When this method returns, the hint if the result is `true`; otherwise, `null`. It is not `null` when the method returns `true`.

#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock for a date without an offset, which is readable only while it is in the future ([TimeProvider.GetUtcNow](https://learn.microsoft.com/dotnet/api/system.timeprovider.getutcnow)); `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system). No other rule of the reader needs a clock.

### Returns
#### [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)
`true` if the error has a readable hint; otherwise, `false`.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.

## Remarks
Both keys are read and combined: [Delay](../RetryAfter/RetryAfter_Delay.md) is the readable value of [RetryAfterSeconds](../ResilienceMetadataKeys.md), and [RetryAt](../RetryAfter/RetryAfter_RetryAt.md) the readable value of [RetryAfterDate](../ResilienceMetadataKeys.md). When both are readable, the date says when the wait ends and the seconds say what the source gave. When neither is, the result is `false`.

| Key | Readable | Not readable |
| --- | --- | --- |
| `retryAfterSeconds` | A whole number of seconds that is not negative and fits a `TimeSpan`, of any integral type (e.g., `Int32` or `Int64`), or a `Decimal`, `Double` or `Single` without a fraction: a deserializer may return any of them for a JSON `120`. | A fraction, a negative value, NaN, an infinity, a value too large, a string, `null`. |
| `retryAfterDate` | An *RFC 3339* text with an upper-case `T`, any fraction of a second, and `Z`, a numeric offset or no offset (e.g., `2026-10-21T07:28:00+00:00`); an HTTP-date in the preferred format of *RFC 9110* (e.g., `Wed, 21 Oct 2026 07:28:00 GMT`); a `DateTimeOffset`; or a `DateTime`. It is returned in UTC. | Any other text, a number, `null`. |

A date that names its offset (`Z`, `GMT`, a numeric offset, a `DateTimeOffset`, a `DateTime` of kind `Utc` or `Local`) is readable also when it has passed: the wait is over, and [GetRemaining(TimeProvider)](../RetryAfter/RetryAfter_GetRemaining.md) gives `TimeSpan.Zero`. A date without an offset (text without one, or a `DateTime` of kind `Unspecified`) is assumed to be UTC, and it is readable only while that instant is in the future.

A key is found whatever its case (e.g., `RetryAfterSeconds` or `RETRYAFTERDATE` taken from a response body). No stored value makes this method throw: the values can come from another service. The full format is in [Metadata format](../../metadata-format.md).

## Examples
```csharp
if (error.TryGetRetryAfter(out var retryAfter))
{
    await Task.Delay(retryAfter.GetRemaining(), cancellationToken);
}
```

## See Also
* [RetryAfter](../RetryAfter/RetryAfter.md)
* [OperationResultResilienceExtensions.GetRetryAfter(IOperationResult, TimeProvider)](../OperationResultResilienceExtensions/OperationResultResilienceExtensions_GetRetryAfter.md)
* [Metadata format](../../metadata-format.md)
* [Retry hint](../../guides/retry-hint.md)
