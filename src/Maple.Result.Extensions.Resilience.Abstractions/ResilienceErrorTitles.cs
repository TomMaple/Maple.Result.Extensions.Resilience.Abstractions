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
///     The titles of the errors that a resilience pipeline creates when it rejects an operation (a timeout, an open circuit, a rate limiter),
///     for the <see cref="Error.Title" />.
/// </summary>
/// <remarks>
///     <para>
///         Each title goes with the type of the same name in <see cref="ResilienceErrorUris" />, e.g.,
///         <c>Error.Unavailable(ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen)</c>: <i>RFC 9457</i> (section 3.1.3) expects
///         the same title for every occurrence of a type.
///     </para>
///     <para>What happened in a single case (e.g., which pipeline, or how long the timeout was) goes into the detail.</para>
/// </remarks>
public static class ResilienceErrorTitles
{
    /// <summary>
    ///     “The circuit breaker is open.”, the title of <see cref="ResilienceErrorUris.CircuitOpen" />: a circuit breaker that is open or isolated.
    ///     Its category is <see cref="ErrorCategory.Unavailable" />.
    /// </summary>
    public const string CircuitOpen = "The circuit breaker is open.";

    /// <summary>
    ///     “The rate limit was exceeded.”, the title of <see cref="ResilienceErrorUris.RateLimited" />: a rate limiter that rejected the operation.
    ///     Its category is <see cref="ErrorCategory.LimitExceeded" />.
    /// </summary>
    public const string RateLimited = "The rate limit was exceeded.";

    /// <summary>
    ///     “The operation timed out.”, the title of <see cref="ResilienceErrorUris.Timeout" />: a timeout strategy that ended the operation.
    ///     Its category is <see cref="ErrorCategory.Timeout" />.
    /// </summary>
    public const string Timeout = "The operation timed out.";
}
