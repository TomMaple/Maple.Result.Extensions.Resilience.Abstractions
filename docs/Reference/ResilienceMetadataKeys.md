# ResilienceMetadataKeys Class
## Definition
Namespace: [Maple.Results.Extensions.Resilience.Abstractions](namespace.md)<br>
Assembly: Maple.Result.Extensions.Resilience.Abstractions.dll<br>
Source: <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceMetadataKeys.cs" target="_blank">ResilienceMetadataKeys.cs</a>

The keys of the [Metadata](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error_Metadata.md) entries that describe how an operation that failed can be retried.

```csharp
public static class ResilienceMetadataKeys
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → ResilienceMetadataKeys

## Remarks
The keys and the types and formats of their values are a contract with other packages (e.g., the HTTP packages of Maple.Result, which write the `Retry-After` header of a response) and with non-.NET clients that read a serialized error, so they never change. The format is described in [Metadata format](../metadata-format.md).

An error carries one key of the hint or both. [ErrorBuilderResilienceExtensions](ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions.md) writes the seconds for a delay whose start is not known, the date for a date, and both for a delay that was received: the delay as it was given, and the instant at which the wait ends. When both are present, the date says when the wait ends.

Read the hint with [ErrorResilienceExtensions.TryGetRetryAfter(Error, RetryAfter, TimeProvider)](ErrorResilienceExtensions/ErrorResilienceExtensions_TryGetRetryAfter.md), which accepts more forms of the values and combines the keys. [Error.TryGetMetadata&lt;T&gt;(String, T)](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error_TryGetMetadata.md) does not convert a value: to read a key directly, use the type of the table. A key is found whatever its case, as every lookup of `Maple.Result` does.

## Fields
| Name | Value | Description |
| ---- | ----- | ----------- |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceMetadataKeys.cs#L41" target="_blank">RetryAfterDate</a> | `"retryAfterDate"` | The key of the time when the wait ends: a `String` with a UTC date and time in whole seconds, in the format `yyyy-MM-dd'T'HH:mm:sszzz` (invariant culture), e.g., `2026-10-21T07:28:00+00:00`. The offset is always `+00:00`, never `Z`: by *RFC 9557*, `+00:00` says that UTC is the reference, while `Z` says that the local offset is unknown. The date may be in the past. |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceMetadataKeys.cs#L50" target="_blank">RetryAfterSeconds</a> | `"retryAfterSeconds"` | The key of the delay before the operation is retried, as the source gave it: an `Int64` with the number of whole seconds, from `0` to `922337203685` (the largest number of whole seconds of a `TimeSpan`), e.g., `120`. A delay counts from the moment it was received; when `RetryAfterDate` is present too, it holds that moment plus the delay. |
| <a href="https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/blob/main/src/Maple.Result.Extensions.Resilience.Abstractions/ResilienceMetadataKeys.cs#L59" target="_blank">Retryable</a> | `"retryable"` | The key of the explicit answer whether the operation may be retried: a `Boolean`. `true` makes any error retryable, `false` forbids a retry, also with a retry hint; see [ErrorResilienceExtensions.IsRetryable(Error, TimeProvider)](ErrorResilienceExtensions/ErrorResilienceExtensions_IsRetryable.md). A value that is not a `Boolean` (e.g., the text `"true"`) counts as absent. |

## Examples
```csharp
// The flag, read directly: the type of the table, as TryGetMetadata does not convert
if (error.TryGetMetadata<bool>(ResilienceMetadataKeys.Retryable, out var retryable))
{
    logger.LogInformation("The error says retryable: {Retryable}.", retryable);
}
```

## See Also
* [Metadata format](../metadata-format.md)
* [ErrorBuilderResilienceExtensions](ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions.md)
* [ErrorResilienceExtensions](ErrorResilienceExtensions/ErrorResilienceExtensions.md)
