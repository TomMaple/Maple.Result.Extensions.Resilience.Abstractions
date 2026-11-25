# ErrorResilienceExtensions.WithRetryable(Error, Boolean) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorResilienceExtensions.cs#L219" target="_blank">ErrorResilienceExtensions.cs</a>

Returns a copy of the error that stores whether the operation may be retried; the rules are those of [ErrorBuilderResilienceExtensions.WithRetryable(ErrorBuilder, Boolean)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryable.md).

```csharp
public static Error WithRetryable(this Error error, bool retryable);
```

### Parameters
#### `error` [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
The error.

#### `retryable` [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)
`true` makes the error retryable; `false` forbids a retry, also with a retry hint.

### Returns
#### [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md)
A new `Error` with the flag.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `error` is `null`.

## Remarks
The original error is not changed. The retry hint of the copy is the one of the original.

## Examples
```csharp
// A quota that is used up: retrying this month does not help, whatever the category says
private static readonly Error QuotaUsedUp = Error.LimitExceeded(typeUri, "Monthly quota used up.")
    .WithRetryable(false);
```

## See Also
* [IsRetryable(Error, TimeProvider)](ErrorResilienceExtensions_IsRetryable.md)
* [ErrorBuilderResilienceExtensions.WithRetryable(ErrorBuilder, Boolean)](../ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions_WithRetryable.md)
* [Is the error retryable?](../../guides/retryable.md)
