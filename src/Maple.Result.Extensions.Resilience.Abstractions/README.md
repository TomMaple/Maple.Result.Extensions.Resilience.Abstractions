# Maple.Result.Extensions.Resilience.Abstractions
The contracts shared by the resilience packages of [Maple.Result](https://www.nuget.org/packages/Maple.Result): the **retry hint** of an `Error` (how long to wait before the operation is retried), whether the error **may be retried** at all, and the types and titles of the **errors of a resilience pipeline**.

**Pre-release of 3.0**, for `Maple.Result` 3.0: the API may still change before 3.0.0.

* One format for the hint in the metadata of an `Error`, which every package writes and reads the same way, also a client that is not written in .NET.
* A hint that keeps its meaning: a delay counts from the moment it was received (RFC 9110), so a service that reads the error later waits the time that is really left.
* One answer to “may I retry?”, from an explicit flag, the hint or the category.
* No dependencies but `Maple.Result`: no Polly, no `Microsoft.Extensions.*`.
* Native AOT and trimming compatible.

## Installation
```shellscript
dotnet add package Maple.Result.Extensions.Resilience.Abstractions --prerelease
```

```csharp
using Maple.Results;
using Maple.Results.Extensions.Resilience.Abstractions;
```

## Quick start
Store the hint on an error: a delay that was received, with the time it was received:
```csharp
// 429 Too Many Requests, Retry-After: 120
var error = Error.LimitExceeded(typeUri, "The rate limit was exceeded.")
    .WithRetryAfter(TimeSpan.FromSeconds(120), receivedAt: timeProvider.GetUtcNow());
```

Read it back from the error, or from a failed `Result` or `Result<T>`:
```csharp
if (result.IsRetryableError() && result.GetRetryAfter() is { } retryAfter)
{
    await Task.Delay(retryAfter.GetRemaining(), cancellationToken);
}
```

Write it into a `Retry-After` header:
```csharp
if (result.GetRetryAfter() is { } retryAfter)
{
    response.Headers.RetryAfter = retryAfter.GetRemainingSeconds().ToString(CultureInfo.InvariantCulture);
}
```

## Documentation
* [Retry hint](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/docs/guides/retry-hint.md) — storing and reading how long to wait before the operation is retried
* [Is the error retryable?](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/docs/guides/retryable.md) — the rules of `IsRetryable`, and the `retryable` flag
* [Errors of a resilience pipeline](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/docs/guides/pipeline-errors.md) — the types and titles of a timeout, an open circuit and a rate limiter
* [Metadata format](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/docs/metadata-format.md) — the keys `retryAfterSeconds`, `retryAfterDate` and `retryable`, their types and formats
* [API reference](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/docs/Reference/namespace.md)
* [Release notes](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/releases)

## Related packages
* [Maple.Result](https://www.nuget.org/packages/Maple.Result) — the *Result* pattern for .NET, which this package extends.
* [Maple.Result.Extensions.Resilience](https://github.com/TomMaple/Maple.Result.Extensions.Resilience) — helpers for the retry pipeline of an application, with Polly behind them (coming soon).

## License
[MIT](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/LICENSE)
