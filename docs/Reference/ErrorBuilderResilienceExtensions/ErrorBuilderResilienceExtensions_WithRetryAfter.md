# ErrorBuilderResilienceExtensions.WithRetryAfter Methods
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>

Stores the retry hint of the error: how long to wait before the operation is retried, or until when.

## Overloads
| Name | Description |
| ---- | ----------- |
| [WithRetryAfter(ErrorBuilder, DateTimeOffset)](#withretryaftererrorbuilder-datetimeoffset) | Stores the earliest time to retry the operation, and removes the delay. |
| [WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)](#withretryaftererrorbuilder-timespan-datetimeoffset) | Stores a delay that was received, with the time it was received: the delay, and when the wait ends. |
| [WithRetryAfter(ErrorBuilder, TimeSpan)](#withretryaftererrorbuilder-timespan) | Stores the delay before the operation is retried, in whole seconds, and removes the date. |

## Remarks
Every overload replaces the hint: it stores its key or keys, and removes the other one. A stored key replaces an entry whatever the case of its key (e.g., `RetryAfterSeconds` taken from a response body), and the keys are stored in the spelling of [ResilienceMetadataKeys](../ResilienceMetadataKeys.md). The other metadata, and the [Retryable](../ResilienceMetadataKeys.md) flag, are not changed.

The arguments are checked before the builder is changed: a call that throws leaves the builder as it was.


## WithRetryAfter(ErrorBuilder, DateTimeOffset)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorBuilderResilienceExtensions.cs#L116" target="_blank">ErrorBuilderResilienceExtensions.cs</a>

Stores the earliest time to retry the operation under [RetryAfterDate](../ResilienceMetadataKeys.md), and removes the delay of [RetryAfterSeconds](../ResilienceMetadataKeys.md).

```csharp
public static ErrorBuilder WithRetryAfter(this ErrorBuilder builder, DateTimeOffset retryAt);
```

### Parameters
#### `builder` [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The builder of the error.

#### `retryAt` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)
The time; it may be in the past and have any offset. It is converted to UTC, and a fraction of a second is rounded up to the next whole second, so the hint is never earlier than asked; only a time in the last second before [DateTimeOffset.MaxValue](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.maxvalue) loses its fraction instead.

### Returns
#### [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The same `builder` instance.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `builder` is `null`.

### Remarks
The value is a `String` in the format `yyyy-MM-dd'T'HH:mm:sszzz` (invariant culture) with the offset `+00:00`, e.g., `2026-10-21T07:28:00+00:00`. See [Metadata format](../../metadata-format.md#retryafterdate).

### Examples
```csharp
// Retry-After: Wed, 21 Oct 2026 07:28:00 GMT
var error = ErrorBuilder.Unavailable(typeUri, "The service is unavailable.")
    .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero))
    .Build();
```


## WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorBuilderResilienceExtensions.cs#L162" target="_blank">ErrorBuilderResilienceExtensions.cs</a>

Stores a delay that was received, with the time it was received: the delay under [RetryAfterSeconds](../ResilienceMetadataKeys.md), and when the wait ends under [RetryAfterDate](../ResilienceMetadataKeys.md).

```csharp
public static ErrorBuilder WithRetryAfter(this ErrorBuilder builder, TimeSpan delay, DateTimeOffset receivedAt);
```

### Parameters
#### `builder` [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The builder of the error.

#### `delay` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
The delay as it was received (e.g., `Retry-After: 120`); zero or more. A fraction of a second is rounded up to the next whole second, and at most `922337203685` is stored, as by [WithRetryAfter(ErrorBuilder, TimeSpan)](#withretryaftererrorbuilder-timespan).

#### `receivedAt` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)
When the delay was received (e.g., the time of the response); it may be in the past and have any offset. A delay counts from this moment (*RFC 9110*, section 10.2.3).

### Returns
#### [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The same `builder` instance.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `builder` is `null`.

#### [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception)
The `delay` is negative; the builder is unchanged.

### Remarks
The seconds are stored as [WithRetryAfter(ErrorBuilder, TimeSpan)](#withretryaftererrorbuilder-timespan) stores them, and the date as [WithRetryAfter(ErrorBuilder, DateTimeOffset)](#withretryaftererrorbuilder-datetimeoffset) stores it: `receivedAt` plus those whole seconds. A reader that comes later (e.g., a second retry pipeline, an orchestrator, or a service that passes the hint on) gets the time that is really left, not the full delay again; the delay as it was received stays.

Where a delay is received (a response, a rejection), this is the right overload. It is wrong for an error that is reused (e.g., a static field): its instant passes; use [WithRetryAfter(ErrorBuilder, TimeSpan)](#withretryaftererrorbuilder-timespan) there.

When the sum is beyond [DateTimeOffset.MaxValue](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.maxvalue), only the seconds are stored, and an existing date is removed: there is no instant to store. The method does not throw for it, because it runs while a failure is handled.

### Examples
```csharp
// Retry-After: 120, received now: the wait ends 120 s after the response
var error = ErrorBuilder.LimitExceeded(typeUri, "The rate limit was exceeded.")
    .WithRetryAfter(TimeSpan.FromSeconds(120), receivedAt: timeProvider.GetUtcNow())
    .Build();
```


## WithRetryAfter(ErrorBuilder, TimeSpan)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorBuilderResilienceExtensions.cs#L87" target="_blank">ErrorBuilderResilienceExtensions.cs</a>

Stores the delay before the operation is retried, in whole seconds, under [RetryAfterSeconds](../ResilienceMetadataKeys.md), and removes the date of [RetryAfterDate](../ResilienceMetadataKeys.md).

```csharp
public static ErrorBuilder WithRetryAfter(this ErrorBuilder builder, TimeSpan delay);
```

### Parameters
#### `builder` [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The builder of the error.

#### `delay` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
The delay; zero or more. A fraction of a second is rounded up to the next whole second, so the hint is never earlier than asked (e.g., 1.2 seconds are stored as `2L`); at most `922337203685L` is stored, the largest number of whole seconds of a `TimeSpan`.

### Returns
#### [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The same `builder` instance.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `builder` is `null`.

#### [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception)
The `delay` is negative; the builder is unchanged.

### Remarks
This stores a delay whose start is not known, so a reader counts it from the moment it reads: it is the right overload for an error that is created once and used many times (e.g., a static field). Where a delay is received (e.g., in a response), use [WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)](#withretryaftererrorbuilder-timespan-datetimeoffset), which also stores when the wait ends.

The value is an `Int64`. See [Metadata format](../../metadata-format.md#retryafterseconds).

### Examples
```csharp
// An error of the application, reused: a reader counts the delay from the moment it reads
private static readonly Error Busy = ErrorBuilder.Unavailable(typeUri, "The service is busy.")
    .WithRetryAfter(TimeSpan.FromSeconds(30))
    .Build();
```


## See Also
* [ErrorBuilderResilienceExtensions](ErrorBuilderResilienceExtensions.md)
* [ErrorResilienceExtensions.WithRetryAfter](../ErrorResilienceExtensions/ErrorResilienceExtensions_WithRetryAfter.md), the same on an `Error`
* [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md)
* [Retry hint](../../guides/retry-hint.md)
* [RFC 9110, section 10.2.3: Retry-After](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3)
