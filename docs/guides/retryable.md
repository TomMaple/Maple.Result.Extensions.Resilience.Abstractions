# Is the Error Retryable?

How to tell whether an error says that the operation may be retried, and how an error says it explicitly. A retry pipeline asks it before it waits for the [retry hint](retry-hint.md).

## Contents
* [Asking the question](#asking-the-question)
* [The rules](#the-rules)
* [Setting the flag](#setting-the-flag)
* [The error, not the operation](#the-error-not-the-operation)

## Asking the question
`IsRetryable` on an `Error`, or `IsRetryableError` on a `Result` or `Result<T>`, tells whether the error says that the operation may be retried:
```csharp
if (result.IsRetryableError())
{
    // retry, if the operation is safe to repeat
}
```

A successful result is not retryable: `IsRetryableError` is `false`.

See more: [ErrorResilienceExtensions.IsRetryable](../Reference/ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md), [OperationResultResilienceExtensions.IsRetryableError](../Reference/OperationResultResilienceExtensions/OperationResultResilienceExtensions_IsRetryableError.md)

## The rules
The first rule that matches gives the answer:

| # | Rule | Result |
| --- | --- | --- |
| 1 | The flag `retryable` is a `bool` | its value: `true` makes any error retryable, `false` forbids a retry, also with a retry hint |
| 2 | The retry hint is readable (`TryGetRetryAfter`) | `true`, whatever the category (e.g., a `Validation` error of a 413 with `Retry-After`) |
| 3 | The category is `Unavailable`, `Timeout` or `LimitExceeded` | `true` |
| 4 | Otherwise | `false`, also for `Critical` and `NotImplemented` |

* **The flag wins**, because it is the only explicit answer, in both directions, as the `Stripe-Should-Retry` and `x-should-retry` headers of other APIs are. The key is found whatever its case (e.g., `Retryable` taken from a response body), and a value that is not a `bool` (e.g., the text `"true"`) counts as absent.
* **A readable hint** says how long to wait, so the condition is temporary: *RFC 9110* defines `Retry-After` for any follow-up request (section 10.2.3), and a `Retry-After` on a `413 Content Too Large` says that the condition is temporary (section 15.5.14).
* **The categories** are the temporary ones: a service that is unavailable (`503`, section 15.6.4), an operation that took too long (`408` may be repeated, section 15.5.9), a limit that will reset (`429`, *RFC 6585*).
* **`Critical` and `NotImplemented` are not retryable by their category**: the server should say whether a condition is temporary or permanent (*RFC 9110*, sections 15.5 and 15.6), and a `500` alone does not. A server that knows that its error is temporary says so with the flag or a hint.

Pass a `TimeProvider` to set the clock of rule 2 for a date without an offset, which is readable only while it is in the future.

## Setting the flag
Set the flag with `WithRetryable` when the category or the hint would give the wrong answer:
```csharp
// A quota that is used up: retrying this month does not help, whatever the category says
var quotaUsedUp = Error.LimitExceeded(typeUri, "Monthly quota used up.")
    .WithRetryable(false);

// A service that is being deployed rejects the request as invalid for a few seconds
var deploying = ErrorBuilder.Validation(typeUri, "The schema is being updated.")
    .WithRetryable(true)
    .Build();
```

The flag is stored as a `bool` under `retryable` ([ResilienceMetadataKeys.Retryable](../Reference/ResilienceMetadataKeys.md)). It replaces an entry whatever the case of its key, and the retry hint stays.

See more: [ErrorBuilderResilienceExtensions.WithRetryable](../Reference/ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryable.md)

## The error, not the operation
> [!IMPORTANT]
> The answer describes the **error**. Whether the operation is safe to repeat (idempotent, *RFC 9110*, section 9.2.2) is for the caller to know: e.g., a payment that timed out may have been made.

An HTTP client retries a `GET` after a timeout without a second thought; a `POST` that creates an order needs an idempotency key, or a check of what happened, before it is sent again. `IsRetryable` cannot know which one the caller sent, so it answers only for the error.
