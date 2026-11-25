# Goals
1. Follow standards where possible:
	1. The meaning of `Retry-After` in HTTP ([RFC 9110, section 10.2.3](https://www.rfc-editor.org/rfc/rfc9110#section-10.2.3)): a delay in whole seconds that counts from the moment it was received, or a date.
	1. The formats that any client can read: the date and time of [RFC 3339](https://www.rfc-editor.org/rfc/rfc3339#section-5.6), with the offset `+00:00` that [RFC 9557](https://www.rfc-editor.org/rfc/rfc9557#section-2) gives to UTC as the reference, and the *tag* URIs of [RFC 4151](https://www.rfc-editor.org/rfc/rfc4151) for the error types.
	1. The extension members of a problem details object ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)), as every metadata entry of the core is written.
	1. What is retryable follows the HTTP status codes and what other libraries do: the temporary conditions (503, 408, 429), a hint that says how long a condition lasts, and an explicit flag in both directions.
1. A contract, not an implementation:
	1. The keys, the types and formats of their values, and the error types and titles never change: other packages and non-.NET clients read them.
	1. The values are scalars (a number, a string, a Boolean), so every serializer writes and reads them without knowing this package.
	1. Writers are strict and readers are tolerant: a writer stores one type and one format; a reader accepts every form that a deserializer may return for it, finds a key whatever its case, and never throws for a stored value.
1. Few dependencies:
	1. The package references `Maple.Result` only: no *Polly*, no `Microsoft.Extensions.*`. The HTTP packages of the family reference it without bringing a resilience library to their users.
	1. The logic (the hint, the classification) lives here, as plain functions, so the main package, `Maple.Result.Extensions.Resilience`, is a thin adapter: replacing *Polly* rewrites only the adapter.
1. Keep the meaning of the hint:
	1. A delay that was received keeps its moment, so a reader that comes later waits the time that is really left, not the full delay again.
	1. Code that must not read a clock (e.g., an orchestrator, which is replayed) reads when the wait ends without one; the other readers take a `TimeProvider`.
1. No silent mistakes:
	1. An invalid argument of a writer is rejected where it is passed, with an exception that names the parameter, and the builder is unchanged.
	1. The answer of `IsRetryable` describes the error, and the documentation says what it does not know: whether the operation is safe to repeat.
1. Easy to use:
	1. One call stores the hint, one call reads it; the right overload follows from what the code knows (a received delay, a date, a delay whose start is not known).
	1. The common questions have one member each: how long to wait (`GetRemaining`), what to write into a header (`GetRemainingSeconds`), whether to retry at all (`IsRetryable`).
1. Runs on modern .NET:
	1. Supports *Native AOT* and trimming: the package produces no trimming or AOT warnings.
	1. The members are declared in C# 14 extension blocks, and they compile to static extension methods that every C# version and other .NET languages can call.
1. Consistent with the family: the names, the tag scheme of the error types, the versions and the conventions are those of the core and its companion packages.
