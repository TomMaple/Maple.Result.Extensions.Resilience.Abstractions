# ResilienceErrorTitles Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorTitles.cs" target="_blank">ResilienceErrorTitles.cs</a>

The titles of the errors that a resilience pipeline creates when it rejects an operation (a timeout, an open circuit, a rate limiter), for the [Title](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error_Title.md) of the error.

```csharp
public static class ResilienceErrorTitles
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → ResilienceErrorTitles

## Remarks
Each title goes with the type of the same name in [ResilienceErrorUris](ResilienceErrorUris.md): *RFC 9457* (section 3.1.3) expects the same title for every occurrence of a type. What happened in a single case (e.g., which pipeline, or how long the timeout was) goes into the detail.

## Fields
| Name | Value | Description |
| ---- | ----- | ----------- |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorTitles.cs#L31" target="_blank">CircuitOpen</a> | `"The circuit breaker is open."` | The title of [ResilienceErrorUris.CircuitOpen](ResilienceErrorUris.md): a circuit breaker that is open or isolated. Its category is `Unavailable`. |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorTitles.cs#L37" target="_blank">RateLimited</a> | `"The rate limit was exceeded."` | The title of [ResilienceErrorUris.RateLimited](ResilienceErrorUris.md): a rate limiter that rejected the operation. Its category is `LimitExceeded`. |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorTitles.cs#L43" target="_blank">Timeout</a> | `"The operation timed out."` | The title of [ResilienceErrorUris.Timeout](ResilienceErrorUris.md): a timeout strategy that ended the operation. Its category is `Timeout`. |

## Examples
```csharp
var error = ErrorBuilder.Timeout(ResilienceErrorUris.Timeout, ResilienceErrorTitles.Timeout)
    .WithDetail("The call to the inventory service took longer than 10 seconds.")
    .Build();
```

## See Also
* [ResilienceErrorUris](ResilienceErrorUris.md)
* [Errors of a resilience pipeline](../guides/pipeline-errors.md)
* [RFC 9457, section 3.1.3: title](https://www.rfc-editor.org/rfc/rfc9457#section-3.1.3)
