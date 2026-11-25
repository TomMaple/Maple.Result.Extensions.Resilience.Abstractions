[![NuGet Version](https://img.shields.io/nuget/vpre/Maple.Result.Extensions.Resilience.Abstractions?label=Maple.Result.Extensions.Resilience.Abstractions%20%E2%80%94%20nuget)](https://www.nuget.org/packages/Maple.Result.Extensions.Resilience.Abstractions/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Maple.Result.Extensions.Resilience.Abstractions)](https://www.nuget.org/packages/Maple.Result.Extensions.Resilience.Abstractions/)
[![CI](https://img.shields.io/github/actions/workflow/status/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/ci.yml?branch=main&label=CI)](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/actions/workflows/ci.yml?query=branch%3Amain)
[![GitHub last commit](https://img.shields.io/github/last-commit/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/main)](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/commits/main/)
[![License: MIT](https://img.shields.io/github/license/TomMaple/Maple.Result.Extensions.Resilience.Abstractions)](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/LICENSE)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/badge)](https://scorecard.dev/viewer/?uri=github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions)

# Maple.Result.Extensions.Resilience.Abstractions
The contracts shared by the resilience packages of [Maple.Result](https://github.com/TomMaple/Maple.Result): the **retry hint** of an `Error` (how long to wait before the operation is retried), whether the error **may be retried** at all, and the types and titles of the **errors of a resilience pipeline**.

Check also:
* [![Maple.Result](https://img.shields.io/nuget/vpre/Maple.Result?label=Maple.Result%20%E2%80%94%20nuget)](https://www.nuget.org/packages/Maple.Result/)
* [![Maple.Result.Extensions.Resilience](https://img.shields.io/badge/Maple.Result.Extensions.Resilience%20%E2%80%94%20nuget-coming%20soon-lightgrey)](https://github.com/TomMaple/Maple.Result.Extensions.Resilience)

> [!IMPORTANT]
> The package is a **pre-release of 3.0**, for `Maple.Result` 3.0. Its API may still change before 3.0.0; a release candidate lists its changes in the [releases](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/releases).

## Contents
* [Why this package?](#why-this-package)
* [Installation](#installation)
* [Quick start](#quick-start)
* [Features](#features)
* [Related packages](#related-packages)
* [FAQ](#faq)
* [Documentation](#documentation)
* [Give it a star ⭐](#give-it-a-star-)
* [Contributing](#contributing)
* [License](#license)

## Why this package?
A failed operation often says when to try again: a `429 Too Many Requests` or a `503 Service Unavailable` with a `Retry-After` header, a rate limiter that rejects a call. The packages of Maple.Result turn such a response into an `Error` and back, so they have to agree on how the error carries the hint. This package is that agreement:
* **one format** for the hint in the metadata of an `Error`, which every package writes and reads the same way, also a client that is not written in .NET ([Metadata format](docs/metadata-format.md)),
* **a hint that keeps its meaning**: a delay counts from the moment it was received (*RFC 9110*), so a service that reads the error later waits the time that is really left, not the full delay again,
* **one answer to “may I retry?”**, from an explicit flag, the hint or the category,
* **the errors of a resilience pipeline** (a timeout, an open circuit, a rate limiter), recognisable without a reference to *Polly*,
* **no dependencies** but `Maple.Result`: no *Polly*, no `Microsoft.Extensions.*`, so an HTTP package can reference it without bringing a resilience library along,
* Native AOT and trimming compatibility.

## Installation
Add the `Maple.Result.Extensions.Resilience.Abstractions` package to your project using the Package Manager in your IDE or the *dotnet* tool in the console:

```shellscript
dotnet add package Maple.Result.Extensions.Resilience.Abstractions --prerelease
```

The types and the extension methods are in one namespace:

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

## Features
* **[Retry hint](docs/guides/retry-hint.md)** — `WithRetryAfter` for a delay that was received, a date, or a delay whose start is not known; `TryGetRetryAfter` and `GetRetryAfter`, which return a `RetryAfter` with the time that is left (`GetRemaining`), the value for a header (`GetRemainingSeconds`) and when the wait ends (`RetryAt`).
* **[Is the error retryable?](docs/guides/retryable.md)** — `IsRetryable` and `IsRetryableError`: the `retryable` flag, the hint or the category; `WithRetryable` to say it explicitly.
* **[Errors of a resilience pipeline](docs/guides/pipeline-errors.md)** — `ResilienceErrorUris` and `ResilienceErrorTitles` for a timeout, an open circuit and a rate limiter.
* **[Metadata format](docs/metadata-format.md)** — the keys `retryAfterSeconds`, `retryAfterDate` and `retryable`, their types and formats, and what the readers accept.

## Related packages
| Package | Purpose |
| ------- | ------- |
| [Maple.Result](https://github.com/TomMaple/Maple.Result) | The *Result* pattern for .NET: `Result`, `Result<T>` and `Error`, which this package extends. |
| [Maple.Result.Extensions.Resilience](https://github.com/TomMaple/Maple.Result.Extensions.Resilience) | Helpers for the retry pipeline of an application, with *Polly* behind them; it builds on this package. **Coming soon** (not on NuGet yet). |
| [Maple.Result.Extensions.HttpClient](https://github.com/TomMaple/Maple.Result.Extensions.HttpClient) | Maps an `HttpClient` response to a *Result*; its version for 3.0 stores the `Retry-After` header of a response as the hint. |
| [Maple.Result.Extensions.AspNetCore](https://github.com/TomMaple/Maple.Result.Extensions.AspNetCore), [MinimalApi](https://github.com/TomMaple/Maple.Result.Extensions.MinimalApi), [Functions.Worker](https://github.com/TomMaple/Maple.Result.Extensions.Functions.Worker) | Map a *Result* to an HTTP response; their versions for 3.0 write the hint as the `Retry-After` header. |

## FAQ
### Why is this a package of its own?
The HTTP packages need the hint (the keys, a writer, a reader), and an application wants helpers for its retry pipeline, which need *Polly*. One package would push *Polly* into every HTTP package, so there are two: this one with the contracts and the logic, and [Maple.Result.Extensions.Resilience](https://github.com/TomMaple/Maple.Result.Extensions.Resilience), a thin adapter to *Polly*.

### Why are there two keys for one hint?
A delay counts from the moment it was received. The seconds keep what the source gave, and the date keeps when the wait ends, so a reader that comes later knows the time that is left. A plain delay, e.g., of an error in a static field, has no moment, so it is stored without a date. See [Both keys of the hint](docs/metadata-format.md#both-keys-of-the-hint).

### Why is the hint not one object in the metadata?
Every serializer writes and reads a number and a string, also one that does not know this package; an object of a type of this package would need a converter in every service and client. The values are scalars for that reason.

### Why `+00:00` and not `Z`?
By *RFC 9557*, `+00:00` says that UTC is the reference, while `Z` says that the local offset is unknown. Both are valid *RFC 3339*, and the readers accept both.

### Does `IsRetryable` tell me that I can repeat the request?
No. It describes the **error**: whether the condition is temporary. Whether the operation is safe to repeat (idempotent, *RFC 9110*, section 9.2.2) is for the caller to know: e.g., a payment that timed out may have been made. See [The error, not the operation](docs/guides/retryable.md#the-error-not-the-operation).

## Documentation
### Guides
* [Retry hint](docs/guides/retry-hint.md) — storing and reading how long to wait before the operation is retried
* [Is the error retryable?](docs/guides/retryable.md) — the rules of `IsRetryable`, and the `retryable` flag
* [Errors of a resilience pipeline](docs/guides/pipeline-errors.md) — the types and titles of a timeout, an open circuit and a rate limiter
* [Metadata format](docs/metadata-format.md) — the keys, their types and formats, and what the readers accept
* [Goals](docs/goals.md) — what this package aims for

### Reference
* [Maple.Results.Extensions.Resilience.Abstractions](docs/Reference/namespace.md) — `RetryAfter`, the extension methods, the keys and the error types

### Standards
| Standard | What it defines here |
| --- | --- |
| [RFC 9110, section 10.2.3](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3) | The `Retry-After` header: a delay in whole seconds (`delay-seconds`) or an HTTP-date, for any follow-up request. |
| [RFC 9110, section 5.6.7](https://www.rfc-editor.org/rfc/rfc9110#section-5.6.7) | The HTTP-date of the header, e.g., `Wed, 21 Oct 2026 07:28:00 GMT`. |
| [RFC 9110, section 15.6.4](https://www.rfc-editor.org/rfc/rfc9110#section-15.6.4) and [section 15.5.14](https://www.rfc-editor.org/rfc/rfc9110#section-15.5.14) | `503 Service Unavailable` and `413 Content Too Large`, which may carry the header to say how long the condition lasts. |
| [RFC 6585, section 4](https://www.rfc-editor.org/rfc/rfc6585#section-4) | `429 Too Many Requests`, which may carry the header to say how long to wait. |
| [RFC 3339, section 5.6](https://www.rfc-editor.org/rfc/rfc3339#section-5.6) | The date and time format of `retryAfterDate`. |
| [RFC 9557, section 2](https://www.rfc-editor.org/rfc/rfc9557#section-2) | Why the offset is `+00:00`: it says that UTC is the reference, while `Z` says that the local offset is unknown. |
| [RFC 4151](https://www.rfc-editor.org/rfc/rfc4151) | The *tag* URIs of the error types. |
| [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) | Problem Details for HTTP APIs, which `Error` follows; its metadata, the retry hint included, are the extension members of a problem details object. |

## Give it a star ⭐
Do you like it? Show your support by giving this project a star!

## Contributing
Questions, ideas and contributions are welcome:
* [Discussions](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/discussions) — questions and ideas,
* [Issues](https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/issues/new/choose) — bug reports and feature requests,
* [Contributing guide](https://github.com/TomMaple/.github/blob/main/CONTRIBUTING.md) — how to propose a change, and the [conventions](docs/conventions.md) of this repository, which win over it,
* [Releasing](docs/releasing.md) — how a version is prepared and published,
* [Code of Conduct](https://github.com/TomMaple/.github/blob/main/CODE_OF_CONDUCT.md),
* [Security policy](https://github.com/TomMaple/.github/blob/main/SECURITY.md) — report vulnerabilities privately, never in a public issue.

## License
Maple.Result.Extensions.Resilience.Abstractions is licensed under the [MIT License](LICENSE).
