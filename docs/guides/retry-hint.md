# Retry Hint

How to store on an `Error` how long to wait before the operation is retried, and how to read it back. The hint follows the `Retry-After` header of HTTP (*RFC 9110*, section 10.2.3), and it is stored in the metadata of the error, so it travels with the error: through a `Result`, into a problem details response, and back. See [Metadata format](../metadata-format.md) for the keys and their values.

## Contents
* [Storing the hint](#storing-the-hint)
* [Which overload?](#which-overload)
* [Reading the hint](#reading-the-hint)
* [RetryAfter](#retryafter)
* [What the readers accept](#what-the-readers-accept)
* [Clocks and tests](#clocks-and-tests)

## Storing the hint
`WithRetryAfter` on an `ErrorBuilder` stores the hint and returns the same builder. On an `Error`, which is immutable, it returns a changed copy:
```csharp
using Maple.Results;
using Maple.Results.Extensions.Resilience.Abstractions;

// Retry-After: 120, received now: the wait ends 120 s after the response
var rateLimited = ErrorBuilder.LimitExceeded(typeUri, "The rate limit was exceeded.")
    .WithRetryAfter(TimeSpan.FromSeconds(120), receivedAt: timeProvider.GetUtcNow())
    .Build();

// Retry-After: Wed, 21 Oct 2026 07:28:00 GMT
var unavailable = Error.Unavailable(typeUri, "The service is unavailable.")
    .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));
```

Each call replaces the hint: it stores its key or keys, and removes the other one. A key that differs only in case (e.g., `RetryAfterSeconds` taken from a response body) is replaced too, and the keys are stored in the spelling of [ResilienceMetadataKeys](../Reference/ResilienceMetadataKeys.md). The other metadata, and the `retryable` flag, stay.

See more: [ErrorBuilderResilienceExtensions.WithRetryAfter](../Reference/ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md)

## Which overload?
It depends on what the code knows:

| Written with | When | `retryAfterSeconds` | `retryAfterDate` |
| --- | --- | --- | --- |
| `WithRetryAfter(delay, receivedAt)` | a delay was **received** (a response, a rejection) | the delay | `receivedAt` + the delay |
| `WithRetryAfter(retryAt)` | a date | absent | the date |
| `WithRetryAfter(delay)` | a delay whose start is not known, e.g., an error that is created once and reused | the delay | absent |

A delay counts from the moment it was received (*RFC 9110*). Stored with `receivedAt`, it keeps that moment, so whoever reads the error later (a second retry pipeline, an orchestrator, a service that passes the hint on) gets the time that is really left, not the full delay again.

Don’t use `receivedAt` for an error that is reused, e.g., one in a static field: its instant passes, and the hint would soon say that the wait is over. Store the plain delay there:
```csharp
// An error of the application, reused: a reader counts the delay from the moment it reads
private static readonly Error Busy = Error.Unavailable(typeUri, "The service is busy.")
    .WithRetryAfter(TimeSpan.FromSeconds(30));
```

A fraction of a second is rounded up, so the hint is never earlier than asked. A negative delay throws an `ArgumentOutOfRangeException`, and the builder is unchanged.

## Reading the hint
`TryGetRetryAfter` on an `Error`, or `GetRetryAfter` on a `Result` or `Result<T>`, returns a [RetryAfter](../Reference/RetryAfter/RetryAfter.md):
```csharp
if (error.TryGetRetryAfter(out var retryAfter))
{
    await Task.Delay(retryAfter.GetRemaining(), cancellationToken);
}

RetryAfter? hint = result.GetRetryAfter();  // null for a success, or without a readable hint
```

Both keys are read and combined: `Delay` comes from `retryAfterSeconds`, and `RetryAt` from `retryAfterDate`. When both are stored, the date says when the wait ends.

See more: [ErrorResilienceExtensions.TryGetRetryAfter](../Reference/ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md), [OperationResultResilienceExtensions.GetRetryAfter](../Reference/OperationResultResilienceExtensions/OperationResultResilienceExtensions_GetRetryAfter.md)

## RetryAfter
| Member | What it is | Who uses it |
| --- | --- | --- |
| `GetRemaining()` | The time that is left: until `RetryAt`, and zero once it has passed; without a `RetryAt`, the `Delay` | a retry pipeline |
| `GetRemainingSeconds()` | The same in whole seconds, rounded up | a package that writes a `Retry-After` header |
| `RetryAt` | When the wait ends (UTC); `null` for a delay whose start is not known | code that must not read the system clock, e.g., an orchestrator (a durable timer) |
| `Delay` | The delay as the source gave it; `null` for a date | logging, diagnostics |
| `Kind` | `Delay` or `Date`: which form the source gave | anything that has to tell them apart |

A package that writes an HTTP response puts the hint back into the header:
```csharp
if (result.GetRetryAfter() is { } retryAfter)
{
    response.Headers.RetryAfter = retryAfter.GetRemainingSeconds().ToString(CultureInfo.InvariantCulture);
}
```

See more: [RetryAfter](../Reference/RetryAfter/RetryAfter.md)

## What the readers accept
A hint can come from another service, through JSON, so the readers accept more than the writers store:
* Seconds are readable as a whole number that is not negative, of any integral type, or a `decimal`, `double` or `float` without a fraction: a JSON `120` comes back as any of them, depending on the deserializer. A fraction, a negative value or a string is not readable.
* A date is readable as *RFC 3339* text (`2026-10-21T07:28:00+00:00`, `2026-10-21T07:28:00.5Z`), an HTTP-date (`Wed, 21 Oct 2026 07:28:00 GMT`), a `DateTimeOffset` or a `DateTime`, also when it has passed. A date without an offset (`2026-10-21T07:28:00`, or a `DateTime` of kind `Unspecified`) is assumed to be UTC, and is readable only while it is in the future.
* A key is found whatever its case (e.g., `RetryAfterSeconds` or `RETRYAFTERDATE` taken from a response body).
* No stored value makes the readers throw. A value that is not readable counts as absent.

See more: [Reading the values](../metadata-format.md#reading-the-values)

## Clocks and tests
`GetRemaining`, `GetRemainingSeconds`, and the readers of a date without an offset, read the system clock. Pass a `TimeProvider` to set “now”, e.g., a fake clock in a test (`FakeTimeProvider` of `Microsoft.Extensions.TimeProvider.Testing`), or the clock of the retry pipeline:
```csharp
var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 21, 7, 27, 30, TimeSpan.Zero));

var remaining = result.GetRetryAfter(timeProvider)?.GetRemaining(timeProvider);  // 30 seconds before 07:28:00
```

Code that must not read a clock at all, e.g., an orchestrator, which is replayed, uses `RetryAt` as it is, as the time of a durable timer.
