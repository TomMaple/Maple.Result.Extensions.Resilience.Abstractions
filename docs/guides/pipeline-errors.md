# Errors of a Resilience Pipeline

The types and titles of the errors that a resilience pipeline creates when it rejects an operation: a timeout, an open circuit, a rate limiter. They live in this package, so that a service or another package recognises such an error without a reference to *Polly*.

## Contents
* [The errors](#the-errors)
* [Recognising an error](#recognising-an-error)
* [Creating an error](#creating-an-error)

## The errors
The main package, `Maple.Result.Extensions.Resilience`, creates them from the rejections of a pipeline:

| Member | Type ([ResilienceErrorUris](../Reference/ResilienceErrorUris.md)) | Title ([ResilienceErrorTitles](../Reference/ResilienceErrorTitles.md)) | Category | Created for |
| --- | --- | --- | --- | --- |
| `Timeout` | `tag:mapledev.engineer,2026-03-03:result.resilience.timeout` | The operation timed out. | `Timeout` | a timeout strategy that ended the operation |
| `CircuitOpen` | `tag:mapledev.engineer,2026-03-03:result.resilience.circuitOpen` | The circuit breaker is open. | `Unavailable` | a circuit breaker that is open or isolated |
| `RateLimited` | `tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited` | The rate limit was exceeded. | `LimitExceeded` | a rate limiter that rejected the operation |

The types are *URI Tags* (*RFC 4151*), in the scheme of the Maple.Result packages: `tag:mapledev.engineer,2026-03-03:result.<package>.<name>`. They are a contract with other packages and with non-.NET clients that read a serialized error, so they do not change.

All three categories are retryable by rule 3 of [IsRetryable](retryable.md#the-rules).

## Recognising an error
Compare the type, which identifies the kind of problem (*RFC 9457*, section 3.1.1), not the title:
```csharp
if (error.TypeUri == ResilienceErrorUris.CircuitOpen)
{
    // the operation was not attempted
}
```

A service that reads the error from a problem details response gets the same type back, so the comparison works across services too.

## Creating an error
A type goes with the title of the same name, and with the category of its row:
```csharp
var circuitOpen = Error.Unavailable(ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen);

var timedOut = ErrorBuilder.Timeout(ResilienceErrorUris.Timeout, ResilienceErrorTitles.Timeout)
    .WithDetail("The call to the inventory service took longer than 10 seconds.")
    .Build();
```

*RFC 9457* (section 3.1.3) expects the same title for every occurrence of a type, so what happened in a single case (which pipeline, how long the timeout was) goes into the detail. A rejection that says how long to wait stores it as the [retry hint](retry-hint.md):
```csharp
var rateLimited = Error.LimitExceeded(ResilienceErrorUris.RateLimited, ResilienceErrorTitles.RateLimited)
    .WithRetryAfter(retryAfter, receivedAt: timeProvider.GetUtcNow());
```

An error of your own application, with a meaning of its own, gets a type and a title of its own: these three are for the rejections of a resilience pipeline.
