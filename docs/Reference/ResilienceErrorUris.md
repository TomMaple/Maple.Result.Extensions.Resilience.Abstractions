# ResilienceErrorUris Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorUris.cs" target="_blank">ResilienceErrorUris.cs</a>

The type URIs of the errors that a resilience pipeline creates when it rejects an operation (a timeout, an open circuit, a rate limiter): *URI Tags* (*RFC 4151*).

```csharp
public static class ResilienceErrorUris
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → ResilienceErrorUris

## Remarks
A service or another package recognises such an error by its type, without a reference to *Polly*, e.g., `error.TypeUri == ResilienceErrorUris.CircuitOpen`. The main package, `Maple.Result.Extensions.Resilience`, creates these errors.

Pair each one with the title of the same name in [ResilienceErrorTitles](ResilienceErrorTitles.md) and the category of its row. The types are a contract with other packages and with non-.NET clients that read a serialized error, so they do not change.

## Properties
| Name | Value | Description |
| ---- | ----- | ----------- |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorUris.cs#L37" target="_blank">CircuitOpen</a> | `tag:mapledev.engineer,2026-03-03:result.resilience.circuitOpen` | The type of an error of a circuit breaker that is open or isolated, so the operation was not attempted. Its category is `Unavailable`, and its title [ResilienceErrorTitles.CircuitOpen](ResilienceErrorTitles.md). |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorUris.cs#L44" target="_blank">RateLimited</a> | `tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited` | The type of an error of a rate limiter that rejected the operation. Its category is `LimitExceeded`, and its title [ResilienceErrorTitles.RateLimited](ResilienceErrorTitles.md). |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceErrorUris.cs#L50" target="_blank">Timeout</a> | `tag:mapledev.engineer,2026-03-03:result.resilience.timeout` | The type of an error of a timeout strategy that ended the operation. Its category is `Timeout`, and its title [ResilienceErrorTitles.Timeout](ResilienceErrorTitles.md). |

Each property is an [ErrorUri](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorUri/ErrorUri.md) created once with [ErrorUri.Tag(String)](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorUri/ErrorUri_Tag.md): `public static ErrorUri CircuitOpen { get; }`.

## Examples
```csharp
var error = Error.Unavailable(ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen);

if (error.TypeUri == ResilienceErrorUris.CircuitOpen)
{
    // the operation was not attempted
}
```

## See Also
* [ResilienceErrorTitles](ResilienceErrorTitles.md)
* [Errors of a resilience pipeline](../guides/pipeline-errors.md)
* [RFC 4151: The 'tag' URI Scheme](https://www.rfc-editor.org/rfc/rfc4151)
