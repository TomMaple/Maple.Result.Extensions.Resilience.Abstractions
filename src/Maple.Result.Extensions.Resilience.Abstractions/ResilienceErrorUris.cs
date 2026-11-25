// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     The type <i>URI</i>s of the errors that a resilience pipeline creates when it rejects an operation (a timeout, an open circuit, a rate
///     limiter): <i>URI Tag</i>s (<i>RFC 4151</i>).
/// </summary>
/// <remarks>
///     <para>
///         A service or another package recognises such an error by its type, without a reference to Polly, e.g.,
///         <c>error.TypeUri == ResilienceErrorUris.CircuitOpen</c>.
///     </para>
///     <para>
///         Pair each one with the title of the same name in <see cref="ResilienceErrorTitles" /> and the category that its summary names, e.g.,
///         <c>Error.Unavailable(ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen)</c>.
///     </para>
///     <para>
///         The types are a contract with other packages and with non-.NET clients that read a serialized error, so they do not change.
///     </para>
/// </remarks>
public static class ResilienceErrorUris
{
    /// <summary>
    ///     The type of an error of a circuit breaker that is open or isolated, so the operation was not attempted:
    ///     <c>tag:mapledev.engineer,2026-03-03:result.resilience.circuitOpen</c>. Its category is <see cref="ErrorCategory.Unavailable" />, and
    ///     its title <see cref="ResilienceErrorTitles.CircuitOpen" />.
    /// </summary>
    public static ErrorUri CircuitOpen { get; } = ErrorUri.Tag("tag:mapledev.engineer,2026-03-03:result.resilience.circuitOpen");

    /// <summary>
    ///     The type of an error of a rate limiter that rejected the operation:
    ///     <c>tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited</c>. Its category is <see cref="ErrorCategory.LimitExceeded" />, and
    ///     its title <see cref="ResilienceErrorTitles.RateLimited" />.
    /// </summary>
    public static ErrorUri RateLimited { get; } = ErrorUri.Tag("tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited");

    /// <summary>
    ///     The type of an error of a timeout strategy that ended the operation: <c>tag:mapledev.engineer,2026-03-03:result.resilience.timeout</c>.
    ///     Its category is <see cref="ErrorCategory.Timeout" />, and its title <see cref="ResilienceErrorTitles.Timeout" />.
    /// </summary>
    public static ErrorUri Timeout { get; } = ErrorUri.Tag("tag:mapledev.engineer,2026-03-03:result.resilience.timeout");
}
