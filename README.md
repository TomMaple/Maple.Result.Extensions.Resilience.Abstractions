# Maple.Result.Extensions.Resilience.Abstractions

Abstractions for the resilience extensions of [Maple.Result](https://www.nuget.org/packages/Maple.Result): the contracts
shared by the resilience packages.

The package stores a **retry hint** on an `Error`, so that every package that maps an error agrees on the names and the
value types: e.g., `Maple.Result.Extensions.HttpClient` stores the `Retry-After` header of a response, and the packages
that write a response can put it back into the header.

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
Store the delay, or the earliest time, to retry the operation with `WithRetryAfter` on an `ErrorBuilder`:
```csharp
// Retry-After: 120
var rateLimited = ErrorBuilder.LimitExceeded(typeUri, "The rate limit was exceeded.")
    .WithRetryAfter(TimeSpan.FromSeconds(120))
    .Build();

// Retry-After: Wed, 21 Oct 2026 07:28:00 GMT
var unavailable = ErrorBuilder.Unavailable(typeUri, "The service is unavailable.")
    .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero))
    .Build();
```

An error carries at most one hint: each call stores its own key and removes the other. It also removes the keys that
differ from them only in case (e.g., `RetryAfterSeconds` taken from a response body), which `ErrorBuilder.WithMetadata`
would reject.

## Metadata keys
The keys are in `ResilienceMetadataKeys`. `Error.TryGetMetadata<T>` does not convert a value, so read each key with its
type:

| Key | Type | Value |
| --- | --- | --- |
| `retryAfterSeconds` | `long` | The delay in whole seconds, zero or more; a fraction is rounded up (1.2 s → `2`). |
| `retryAfterDate` | `string` | The UTC time in whole seconds, `yyyy-MM-dd'T'HH:mm:sszzz`, e.g., `2026-10-21T07:28:00+00:00`; a fraction is rounded up. The offset is always `+00:00`, never `Z`. |

```csharp
if (rateLimited.TryGetMetadata<long>(ResilienceMetadataKeys.RetryAfterSeconds, out var seconds))
{
    // seconds == 120
}
```

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
