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

## License

[MIT](LICENSE)
