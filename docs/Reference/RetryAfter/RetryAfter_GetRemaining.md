# RetryAfter.GetRemaining(TimeProvider) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/RetryAfter.cs#L96" target="_blank">RetryAfter.cs</a>

Returns the time that is left until the operation can be retried.

```csharp
public TimeSpan GetRemaining(TimeProvider? timeProvider = null);
```

### Parameters
#### `timeProvider` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)
The clock that gives “now” ([TimeProvider.GetUtcNow](https://learn.microsoft.com/dotnet/api/system.timeprovider.getutcnow)); `null` means [TimeProvider.System](https://learn.microsoft.com/dotnet/api/system.timeprovider.system).

### Returns
#### [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
With a [RetryAt](RetryAfter_RetryAt.md): the time from now until then, and `TimeSpan.Zero` once it has passed. Without one: [Delay](RetryAfter_Delay.md), whatever the clock says.

## Remarks
Pass a `TimeProvider` to set “now”, e.g., a fake clock in a test, or the clock of the retry pipeline.

## Examples
```csharp
await Task.Delay(retryAfter.GetRemaining(timeProvider), timeProvider, cancellationToken);
```

## See Also
* [GetRemainingSeconds(TimeProvider)](RetryAfter_GetRemainingSeconds.md)
* [RetryAfter](RetryAfter.md)
