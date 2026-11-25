# Maple.Result.Extensions.Resilience.Abstractions

Abstractions for the resilience extensions of [Maple.Result](https://www.nuget.org/packages/Maple.Result): the contracts
shared by the resilience packages.

The package stores a **retry hint** on an `Error`, so that every package that maps an error agrees on the names and the
value types: e.g., `Maple.Result.Extensions.HttpClient` stores the `Retry-After` header of a response, and the packages
that write a response can put it back into the header.

It also says whether an error may be retried at all (`IsRetryable`), from an explicit flag, the hint or the category.

* It references only `Maple.Result`: no Polly, no `Microsoft.Extensions.*`.
* Native AOT and trimming compatible.

## Getting started
```shellscript
dotnet add package Maple.Result.Extensions.Resilience.Abstractions --prerelease
```

```csharp
using Maple.Results;
using Maple.Results.Extensions.Resilience.Abstractions;
```

## Usage
### Storing the hint
`WithRetryAfter` on an `ErrorBuilder`, or on an `Error` (which returns a changed copy), stores the hint. Which overload
to use depends on what the code knows:

| Written with | When | `retryAfterSeconds` | `retryAfterDate` |
| --- | --- | --- | --- |
| `WithRetryAfter(delay, receivedAt)` | a delay was **received** (a response, a rejection) | the delay | `receivedAt` + the delay |
| `WithRetryAfter(retryAt)` | a date | absent | the date |
| `WithRetryAfter(delay)` | a delay whose start is not known, e.g., an error that is created once and reused | the delay | absent |

```csharp
// Retry-After: 120, received now: the wait ends 120 s after the response
var rateLimited = ErrorBuilder.LimitExceeded(typeUri, "The rate limit was exceeded.")
    .WithRetryAfter(TimeSpan.FromSeconds(120), receivedAt: timeProvider.GetUtcNow())
    .Build();

// Retry-After: Wed, 21 Oct 2026 07:28:00 GMT
var unavailable = ErrorBuilder.Unavailable(typeUri, "The service is unavailable.")
    .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero))
    .Build();

// An error of the application, reused: a reader counts the delay from the moment it reads
private static readonly Error Busy = Error.Unavailable(typeUri, "The service is busy.")
    .WithRetryAfter(TimeSpan.FromSeconds(30));
```

A delay counts from the moment it was received (RFC 9110). Stored with `receivedAt`, it keeps that moment, so whoever
reads the error later (a second retry pipeline, an orchestrator, a service that passes the hint on) gets the time that
is really left, not the full delay again. Don't use `receivedAt` for an error that is reused: its instant passes.

Each call replaces both keys, also the ones that differ from them only in case (e.g., `RetryAfterSeconds` taken from a
response body), and stores them in the spelling of `ResilienceMetadataKeys`.

### Reading the hint
`TryGetRetryAfter` on an `Error`, or `GetRetryAfter` on a `Result` or `Result<T>`, returns a `RetryAfter`:
```csharp
if (error.TryGetRetryAfter(out var retryAfter))
{
    await Task.Delay(retryAfter.GetRemaining(), cancellationToken);
}

RetryAfter? hint = result.GetRetryAfter();  // null for a success, or without a readable hint
```

| Member | What it is | Who uses it |
| --- | --- | --- |
| `GetRemaining()` | The time that is left: until `RetryAt`, and zero once it has passed; without a `RetryAt`, the `Delay` | a retry pipeline |
| `GetRemainingSeconds()` | The same in whole seconds, rounded up | a package that writes a `Retry-After` header |
| `RetryAt` | When the wait ends (UTC); `null` for a delay whose start is not known | code that must not read the system clock, e.g., an orchestrator (a durable timer) |
| `Delay` | The delay as the source gave it; `null` for a date | logging, diagnostics |
| `Kind` | `Delay` or `Date`: which form the source gave | anything that has to tell them apart |

`GetRemaining` and `GetRemainingSeconds` read the system clock; pass a `TimeProvider` to set “now” (e.g., in tests).

* Both keys are read and combined: `Delay` comes from `retryAfterSeconds` and `RetryAt` from `retryAfterDate`. When
  both are stored, the date says when the wait ends.
* Seconds are readable as a whole number that is not negative, of any integral type, or a `decimal`, `double` or
  `float` without a fraction: a JSON `120` comes back as any of them, depending on the deserializer. A fraction, a
  negative value or a string is not readable.
* A date is readable as RFC 3339 text (`2026-10-21T07:28:00+00:00`, `2026-10-21T07:28:00.5Z`), an HTTP-date
  (`Wed, 21 Oct 2026 07:28:00 GMT`), a `DateTimeOffset` or a `DateTime`, also when it has passed. A date without an
  offset (`2026-10-21T07:28:00`, or a `DateTime` of kind `Unspecified`) is assumed to be UTC, and is readable only while
  it is in the future.
* A key is found whatever its case (e.g., `RetryAfterSeconds` or `RETRYAFTERDATE` taken from a response body).
* No stored value makes the readers throw: the values can come from another service.

### Is the error retryable?
`IsRetryable` on an `Error`, or `IsRetryableError` on a `Result` or `Result<T>`, tells whether the error says that the
operation may be retried. The first rule that matches gives the answer:

| # | Rule | Result |
| --- | --- | --- |
| 1 | The flag `retryable` is a `bool` | its value: `true` makes any error retryable, `false` forbids a retry, also with a retry hint |
| 2 | The retry hint is readable (`TryGetRetryAfter`) | `true`, whatever the category (e.g., a `Validation` error of a 413 with `Retry-After`) |
| 3 | The category is `Unavailable`, `Timeout` or `LimitExceeded` | `true` |
| 4 | Otherwise | `false`, also for `Critical` and `NotImplemented` |

Set the flag with `WithRetryable` when the category or the hint would give the wrong answer:
```csharp
// A quota that is used up: retrying this month does not help, whatever the category says
var quotaUsedUp = Error.LimitExceeded(typeUri, "Monthly quota used up.")
    .WithRetryable(false);

if (result.IsRetryableError())
{
    // retry, if the operation is safe to repeat
}
```

The answer describes the **error**. Whether the operation is safe to repeat (idempotent, RFC 9110, section 9.2.2) is for
the caller to know: e.g., a payment that timed out may have been made.

## Metadata keys
The keys are in `ResilienceMetadataKeys`. The writers store these types; to read a key directly, use its type, because
`Error.TryGetMetadata<T>` does not convert a value:

| Key | Type | Value |
| --- | --- | --- |
| `retryAfterSeconds` | `long` | The delay as the source gave it, in whole seconds, from 0 to 922,337,203,685; a fraction is rounded up (1.2 s → `2`). |
| `retryAfterDate` | `string` | When the wait ends: the UTC time in whole seconds, `yyyy-MM-dd'T'HH:mm:sszzz`, e.g., `2026-10-21T07:28:00+00:00`; a fraction is rounded up. The offset is always `+00:00`, never `Z`. |
| `retryable` | `bool` | Whether the operation may be retried, whatever the category and the hint say (`IsRetryable`, rule 1). |

An error can carry both keys: then the date says when the wait ends, and the seconds what the source gave.

The readers find a key whatever its case (e.g., `Retryable` taken from a response body), as every lookup of
`Maple.Result` does; the writers store the spelling of the table.

The keys and their formats are a contract with other packages and with non-.NET clients that read a serialized error,
so they do not change.

## Standards
The retry hint follows the `Retry-After` header of HTTP, and its date is written in the Internet date format:

| Standard | What it defines here |
| --- | --- |
| [RFC 9110, §10.2.3](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3) | The `Retry-After` header: a delay in whole seconds (`delay-seconds`) or an HTTP-date, for any follow-up request. |
| [RFC 9110, §5.6.7](https://www.rfc-editor.org/rfc/rfc9110#section-5.6.7) | The HTTP-date of the header, e.g., `Wed, 21 Oct 2026 07:28:00 GMT`. |
| [RFC 9110, §15.6.4](https://www.rfc-editor.org/rfc/rfc9110#section-15.6.4) and [§15.5.14](https://www.rfc-editor.org/rfc/rfc9110#section-15.5.14) | `503 Service Unavailable` and `413 Content Too Large`, which may carry the header to say how long the condition lasts. |
| [RFC 6585, §4](https://www.rfc-editor.org/rfc/rfc6585#section-4) | `429 Too Many Requests`, which may carry the header to say how long to wait. |
| [RFC 3339, §5.6](https://www.rfc-editor.org/rfc/rfc3339#section-5.6) | The date and time format of `retryAfterDate`. |
| [RFC 9557, §2](https://www.rfc-editor.org/rfc/rfc9557#section-2) | Why the offset is `+00:00`: it says that UTC is the reference, while `Z` says that the local offset is unknown. |
| [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) | Problem Details for HTTP APIs, which `Error` follows; its metadata, the retry hint included, are the extension members of a problem details object. |

## License

[MIT](LICENSE)
