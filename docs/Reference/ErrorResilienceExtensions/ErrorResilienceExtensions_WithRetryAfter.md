# ErrorResilienceExtensions.WithRetryAfter Methods
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>

Returns a copy of the error that stores the retry hint: how long to wait before the operation is retried, or until when.

## Overloads
| Name | Description |
| ---- | ----------- |
| [WithRetryAfter(Error, DateTimeOffset)](#withretryaftererror-datetimeoffset) | Returns a copy of the error that stores the earliest time to retry the operation. |
| [WithRetryAfter(Error, TimeSpan, DateTimeOffset)](#withretryaftererror-timespan-datetimeoffset) | Returns a copy of the error that stores a delay that was received, with the time it was received. |
| [WithRetryAfter(Error, TimeSpan)](#withretryaftererror-timespan) | Returns a copy of the error that stores the delay before the operation is retried. |

## Remarks
The rules are those of [ErrorBuilderResilienceExtensions.WithRetryAfter](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md): every overload replaces the hint, and the other metadata stay. The original error is not changed.


## WithRetryAfter(Error, DateTimeOffset)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L181" target="_blank">ErrorResilienceExtensions.cs</a>

Returns a copy of the error that stores the earliest time to retry the operation; the rules are those of [ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, DateTimeOffset)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-datetimeoffset).

```csharp
public static Error WithRetryAfter(this Error error, DateTimeOffset retryAt);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `retryAt` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)
The time; it may be in the past and have any offset. It is stored in UTC, rounded up to the next whole second.

### Returns
#### [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
A new `Error` with the hint.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.


## WithRetryAfter(Error, TimeSpan, DateTimeOffset)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L200" target="_blank">ErrorResilienceExtensions.cs</a>

Returns a copy of the error that stores a delay that was received, with the time it was received; the rules are those of [ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan-datetimeoffset).

```csharp
public static Error WithRetryAfter(this Error error, TimeSpan delay, DateTimeOffset receivedAt);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `delay` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
The delay as it was received; zero or more. A fraction of a second is rounded up to the next whole second.

#### `receivedAt` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)
When the delay was received; it may be in the past and have any offset.

### Returns
#### [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
A new `Error` with the hint: the delay, and when the wait ends.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.

#### [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception)
The `delay` is negative.

### Examples
```csharp
// A rejection of a rate limiter, with its delay, turned into an error
var error = Error.LimitExceeded(ResilienceErrorUris.RateLimited, ResilienceErrorTitles.RateLimited)
    .WithRetryAfter(retryAfter, receivedAt: timeProvider.GetUtcNow());
```


## WithRetryAfter(Error, TimeSpan)
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L164" target="_blank">ErrorResilienceExtensions.cs</a>

Returns a copy of the error that stores the delay before the operation is retried; the rules are those of [ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md#withretryaftererrorbuilder-timespan).

```csharp
public static Error WithRetryAfter(this Error error, TimeSpan delay);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `delay` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
The delay; zero or more. A fraction of a second is rounded up to the next whole second.

### Returns
#### [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
A new `Error` with the hint.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.

#### [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception)
The `delay` is negative.

### Remarks
This stores a delay whose start is not known; where a delay is received, use [WithRetryAfter(Error, TimeSpan, DateTimeOffset)](#withretryaftererror-timespan-datetimeoffset).

### Examples
```csharp
// An error of the application, reused: a reader counts the delay from the moment it reads
private static readonly Error Busy = Error.Unavailable(typeUri, "The service is busy.")
    .WithRetryAfter(TimeSpan.FromSeconds(30));
```


## See Also
* [ErrorResilienceExtensions](ErrorResilienceExtensions.md)
* [ErrorBuilderResilienceExtensions.WithRetryAfter](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryAfter.md)
* [TryGetRetryAfter(Error, RetryAfter, TimeProvider)](ErrorResilienceExtensions_TryGetRetryAfter.md)
* [Retry hint](../../guides/retry-hint.md)
