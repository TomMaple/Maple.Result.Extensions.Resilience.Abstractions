# RetryAfter.GetRemainingSeconds(TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs#L119" target="_blank">RetryAfter.cs</a>

Returns the time that is left in whole seconds, a fraction rounded up: the value for a `Retry-After` header.

```csharp
public long GetRemainingSeconds(TimeProvider? timeProvider = null);
```

### Parameters
#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock that gives “now” ([TimeProvider.GetUtcNow](https://learn.microsoft.com/dotnet/api/system.timeprovider.getutcnow)); `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system).

### Returns
#### [Int64](https://learn.microsoft.com/dotnet/api/system.int64)
[GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md) in whole seconds, rounded up; zero or more, and at most `922337203685`, the largest number of whole seconds of a `TimeSpan`.

## Remarks
The header takes whole seconds (`delay-seconds`, *RFC 9110*, section 10.2.3). Rounding up keeps the client from retrying before the time is over.

## Examples
```csharp
if (result.GetRetryAfter() is { } retryAfter)
{
    response.Headers.RetryAfter = retryAfter.GetRemainingSeconds().ToString(CultureInfo.InvariantCulture);
}
```

## See Also
* [GetRemaining(TimeProvider)](RetryAfter_GetRemaining.md)
* [RetryAfter](RetryAfter.md)
* [RFC 9110, section 10.2.3: Retry-After](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3)
