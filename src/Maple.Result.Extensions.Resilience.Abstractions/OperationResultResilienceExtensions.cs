// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     Extension methods of <see cref="IOperationResult" /> (<see cref="Result" /> and <see cref="Result{T}" />) that read how a failed operation can be
///     retried.
/// </summary>
public static class OperationResultResilienceExtensions
{
    /// <summary>
    ///     The members that read the retry hint of a failed <see cref="IOperationResult" />, and whether it may be retried.
    /// </summary>
    /// <param name="result">The result.</param>
    extension(IOperationResult result)
    {
        /// <summary>
        ///     Returns the retry hint of a failed result.
        /// </summary>
        /// <param name="timeProvider">
        ///     The clock for a date without an offset, which is readable only while it is in the future (<see cref="TimeProvider.GetUtcNow" />);
        ///     <see langword="null" /> means <see cref="TimeProvider.System" />.
        /// </param>
        /// <returns>
        ///     The hint; <see langword="null" /> for a successful result, and for a failed one without a readable hint. The time that is left is
        ///     <see cref="RetryAfter.GetRemaining" />.
        /// </returns>
        /// <remarks>
        ///     The hint is read as <see cref="ErrorResilienceExtensions.TryGetRetryAfter" /> reads it.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="result" /> is <see langword="null" />.</exception>
        public RetryAfter? GetRetryAfter(TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (!result.TryGetError(out var error))
                return null;

            return error.TryGetRetryAfter(out var retryAfter, timeProvider)
                ? retryAfter
                : null;
        }

        /// <summary>
        ///     Tells whether the result failed with an error that may be retried.
        /// </summary>
        /// <param name="timeProvider">
        ///     The clock for a retry hint with a date without an offset, which is readable only while it is in the future;
        ///     <see langword="null" /> means <see cref="TimeProvider.System" />.
        /// </param>
        /// <returns>
        ///     <see langword="false" /> for a successful result; otherwise <see cref="ErrorResilienceExtensions.IsRetryable" /> of its error.
        /// </returns>
        /// <remarks>
        ///     The answer describes the error. Whether the operation is safe to repeat (idempotent, RFC 9110, section 9.2.2) is for the caller to know.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="result" /> is <see langword="null" />.</exception>
        public bool IsRetryableError(TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.TryGetError(out var error) && error.IsRetryable(timeProvider);
        }
    }
}
