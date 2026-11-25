# ErrorBuilderResilienceExtensions.WithRetryable(ErrorBuilder, Boolean) Method
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](../namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/v3.0.0-rc.2/src/Maple.Result.Extensions.Resilience.Abstractions/ErrorBuilderResilienceExtensions.cs#L199" target="_blank">ErrorBuilderResilienceExtensions.cs</a>

Stores under [Retryable](../ResilienceMetadataKeys.md) whether the operation may be retried, whatever the category and the retry hint say.

```csharp
public static ErrorBuilder WithRetryable(this ErrorBuilder builder, bool retryable);
```

### Parameters
#### `builder` [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The builder of the error.

#### `retryable` [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)
`true` makes the error retryable (e.g., a `Validation` error of a service that is being deployed); `false` forbids a retry, also for an `Unavailable` error and with a retry hint (e.g., a monthly quota that is used up).

### Returns
#### [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md)
The same `builder` instance.

### Exceptions
#### [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)
The `builder` is `null`.

## Remarks
The value is a `Boolean`. It replaces an entry of [Retryable](../ResilienceMetadataKeys.md) whatever the case of its key (e.g., `Retryable` taken from a response body). The retry hint is not changed.

The flag is the first rule of [IsRetryable(Error, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md): set it when the category or the hint would give the wrong answer.

## Examples
```csharp
// A quota that is used up: retrying this month does not help, whatever the category says
var error = ErrorBuilder.LimitExceeded(typeUri, "Monthly quota used up.")
    .WithRetryable(false)
    .Build();
```

## See Also
* [ErrorResilienceExtensions.WithRetryable(Error, Boolean)](../ErrorResilienceExtensions/ErrorResilienceExtensions_WithRetryable.md), the same on an `Error`
* [ErrorResilienceExtensions.IsRetryable(Error, TimeProvider)](../ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md)
* [Is the error retryable?](../../guides/retryable.md)
