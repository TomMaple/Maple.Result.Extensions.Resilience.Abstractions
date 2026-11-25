# Maple.Results.Extensions.Resilience.Abstractions Namespace
Provides the contracts shared by the resilience packages of [Maple.Result](https://github.com/TomMaple/Maple.Result): the retry hint of an [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md) (how long to wait before the operation is retried), whether the error may be retried at all, and the types and titles of the errors that a resilience pipeline creates.

```csharp
using Maple.Results.Extensions.Resilience.Abstractions;
```

## Records
| Name | Description |
| ---- | ----------- |
| [RetryAfter](RetryAfter/RetryAfter.md) | The retry hint of an error: how long to wait before the operation is retried, as the source gave it, and when the wait ends. |

## Classes
| Name | Description |
| ---- | ----------- |
| [ErrorBuilderResilienceExtensions](ErrorBuilderResilienceExtensions/ErrorBuilderResilienceExtensions.md) | Extension methods of [ErrorBuilder](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/ErrorBuilder/ErrorBuilder.md) that store how an operation that failed can be retried, and whether it may be. |
| [ErrorResilienceExtensions](ErrorResilienceExtensions/ErrorResilienceExtensions.md) | Extension methods of [Error](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error.md) that store and read how an operation that failed can be retried, and whether it may be. |
| [OperationResultResilienceExtensions](OperationResultResilienceExtensions/OperationResultResilienceExtensions.md) | Extension methods of [IOperationResult](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/IOperationResult/IOperationResult.md) (`Result` and `Result<T>`) that read how a failed operation can be retried. |
| [ResilienceErrorTitles](ResilienceErrorTitles.md) | The titles of the errors that a resilience pipeline creates when it rejects an operation. |
| [ResilienceErrorUris](ResilienceErrorUris.md) | The type URIs of the errors that a resilience pipeline creates when it rejects an operation: *URI Tags* (*RFC 4151*). |
| [ResilienceMetadataKeys](ResilienceMetadataKeys.md) | The keys of the [Metadata](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/Error/Error_Metadata.md) entries that describe how an operation that failed can be retried. |

## Enums
| Name | Description |
| ---- | ----------- |
| [RetryAfterKind](RetryAfterKind.md) | The form in which the source gave a retry hint. |

## See Also
* [Metadata format](../metadata-format.md)
* [Retry hint](../guides/retry-hint.md)
* [Is the error retryable?](../guides/retryable.md)
* [Errors of a resilience pipeline](../guides/pipeline-errors.md)
* [Maple.Results namespace](https://github.com/TomMaple/Maple.Result/blob/main/docs/Reference/namespace.md), in the core package
