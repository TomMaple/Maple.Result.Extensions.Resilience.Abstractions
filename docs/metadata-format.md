# Metadata Format

How an `Error` carries its retry hint and its `retryable` flag: the keys of its metadata, the types and formats of their values, and what the readers accept. Every package that writes or reads these keys follows this format, so a hint written by one service is read the same way by another, also by a client that is not written in .NET.

The keys are available as constants in [ResilienceMetadataKeys](Reference/ResilienceMetadataKeys.md).

## Contents
* [Example](#example)
* [Keys](#keys)
* [`retryAfterSeconds`](#retryafterseconds)
* [`retryAfterDate`](#retryafterdate)
* [`retryable`](#retryable)
* [Both keys of the hint](#both-keys-of-the-hint)
* [Case of the keys](#case-of-the-keys)
* [Reading the values](#reading-the-values)
* [Standards](#standards)

## Example
A rate limiter answered `429 Too Many Requests` with `Retry-After: 120` at 07:26:00 UTC:
```csharp
var error = ErrorBuilder.LimitExceeded(ResilienceErrorUris.RateLimited, ResilienceErrorTitles.RateLimited)
    .WithRetryAfter(TimeSpan.FromSeconds(120), receivedAt: new DateTimeOffset(2026, 10, 21, 7, 26, 0, TimeSpan.Zero))
    .Build();
```

The metadata are written as the extension members of a problem details object (*RFC 9457*; see [Problem details format](https://github.com/TomMaple/Maple.Result/blob/main/docs/problem-details.md#metadata-extension-members) of the core):
```json
{
  "type": "tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited",
  "title": "The rate limit was exceeded.",
  "status": 429,
  "retryAfterSeconds": 120,
  "retryAfterDate": "2026-10-21T07:28:00+00:00"
}
```

A service that reads this response later, e.g., at 07:27:30, waits the 30 seconds that are left, not 120 seconds again.

## Keys
| Key | Type | Value | Written by |
| --- | --- | --- | --- |
| `retryAfterSeconds` | `long` (JSON number) | The delay as the source gave it, in whole seconds, from 0 to 922,337,203,685 | `WithRetryAfter(delay)`, `WithRetryAfter(delay, receivedAt)` |
| `retryAfterDate` | `string` | When the wait ends: a UTC date and time in whole seconds, `yyyy-MM-dd'T'HH:mm:sszzz` | `WithRetryAfter(retryAt)`, `WithRetryAfter(delay, receivedAt)` |
| `retryable` | `bool` (JSON `true` or `false`) | Whether the operation may be retried, whatever the category and the hint say | `WithRetryable(retryable)` |

The keys, the types and the formats are a contract, so they never change. A writer stores the key in the spelling of the table.

## `retryAfterSeconds`
The delay before the operation is retried, as the source gave it (e.g., `Retry-After: 120`), as a `long`:
* a whole number of seconds: a fraction is rounded up, so the hint is never earlier than asked (1.2 seconds are stored as `2`);
* from `0` to `922337203685`, the largest number of whole seconds of a `TimeSpan`: a longer delay is stored as that number.

A delay counts from the moment it was received (*RFC 9110*, section 10.2.3). Alone, it says nothing about that moment, so a reader counts it from the moment it reads: this is right for an error that is created once and reused (e.g., a static field), and wrong for a delay that was received, which also stores the date.

## `retryAfterDate`
When the wait ends, as a `string` in the format `yyyy-MM-dd'T'HH:mm:sszzz` (invariant culture), e.g., `2026-10-21T07:28:00+00:00`:
* the time is in UTC, and a fraction of a second is rounded up, so the hint is never earlier than asked;
* the offset is always `+00:00`, never `Z`: by *RFC 9557*, section 2, `+00:00` says that UTC is the reference, while `Z` says that the local offset is unknown;
* it is an *RFC 3339* date and time (section 5.6), which any JSON client can parse;
* it may be in the past: the wait is over.

## `retryable`
The explicit answer whether the operation may be retried, as a `bool`. It is the first rule of `IsRetryable` (see [Is the error retryable?](guides/retryable.md)): `true` makes any error retryable, and `false` forbids a retry, also with a retry hint. A value that is not a `bool` (e.g., the text `"true"`) counts as absent.

## Both keys of the hint
An error carries one key of the hint or both:

| Written with | `retryAfterSeconds` | `retryAfterDate` |
| --- | --- | --- |
| `WithRetryAfter(delay, receivedAt)`: a delay was **received** | the delay | `receivedAt` + the delay |
| `WithRetryAfter(retryAt)`: a date | absent | the date |
| `WithRetryAfter(delay)`: a delay whose start is not known | the delay | absent |

When both are present, the date says when the wait ends, and the seconds say what the source gave. Every writer of the hint replaces both keys: it stores its key or keys and removes the other one. When the date of `WithRetryAfter(delay, receivedAt)` would be beyond `DateTimeOffset.MaxValue`, only the seconds are stored.

## Case of the keys
The readers find a key whatever its case (e.g., `RetryAfterSeconds` or `RETRYABLE` taken from a response body), as every lookup of `Maple.Result` does, and a writer replaces an entry whatever the case of its key. An error never holds two spellings of one key (see [Problem details format](https://github.com/TomMaple/Maple.Result/blob/main/docs/problem-details.md#metadata-extension-members) of the core).

## Reading the values
A value comes back from JSON in whatever type the deserializer chose, so the readers accept more than the writers store:

| Key | Readable | Not readable |
| --- | --- | --- |
| `retryAfterSeconds` | A whole number that is not negative and fits a `TimeSpan`: any integral type, or a `decimal`, `double` or `float` without a fraction (a JSON `120` comes back as any of them) | A fraction, a negative value, NaN, an infinity, a value too large, a string, `null` |
| `retryAfterDate` | An *RFC 3339* text with an upper-case `T`, any fraction of a second, and `Z`, a numeric offset or no offset; an HTTP-date in the preferred format of *RFC 9110* (`Wed, 21 Oct 2026 07:28:00 GMT`); a `DateTimeOffset`; a `DateTime` | Any other text, a number, `null` |
| `retryable` | A `bool` | Anything else, e.g., the text `"true"`, `1` or `null` |

* A date that names its offset (`Z`, `GMT`, a numeric offset, a `DateTimeOffset`, a `DateTime` of kind `Utc` or `Local`) is readable also when it has passed: the wait is over.
* A date without an offset (`2026-10-21T07:28:00`, or a `DateTime` of kind `Unspecified`) is assumed to be UTC, and it is readable only while that instant is in the future. This is the only rule of the readers that needs a clock: pass a `TimeProvider` to set it.
* A value that is not readable counts as absent: the other key of the hint may still be readable. No value makes a reader throw, because the values can come from another service.

`Error.TryGetMetadata<T>` of the core does not convert a value: to read a key directly, use the type of the [Keys](#keys) table. [TryGetRetryAfter](Reference/ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md) reads both keys of the hint with the rules above.

## Standards
| Standard | What it defines here |
| --- | --- |
| [RFC 9110, section 10.2.3](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3) | The `Retry-After` header: a delay in whole seconds (`delay-seconds`) or an HTTP-date, for any follow-up request. |
| [RFC 9110, section 5.6.7](https://www.rfc-editor.org/rfc/rfc9110#section-5.6.7) | The HTTP-date of the header, e.g., `Wed, 21 Oct 2026 07:28:00 GMT`. |
| [RFC 9110, section 15.6.4](https://www.rfc-editor.org/rfc/rfc9110#section-15.6.4) and [section 15.5.14](https://www.rfc-editor.org/rfc/rfc9110#section-15.5.14) | `503 Service Unavailable` and `413 Content Too Large`, which may carry the header to say how long the condition lasts. |
| [RFC 6585, section 4](https://www.rfc-editor.org/rfc/rfc6585#section-4) | `429 Too Many Requests`, which may carry the header to say how long to wait. |
| [RFC 3339, section 5.6](https://www.rfc-editor.org/rfc/rfc3339#section-5.6) | The date and time format of `retryAfterDate`. |
| [RFC 9557, section 2](https://www.rfc-editor.org/rfc/rfc9557#section-2) | Why the offset is `+00:00`: it says that UTC is the reference, while `Z` says that the local offset is unknown. |
| [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) | Problem Details for HTTP APIs, which `Error` follows; its metadata, the retry hint included, are the extension members of a problem details object. |
