// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Maple.Results.Extensions.Resilience.Abstractions.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     Extension methods of <see cref="Error" /> that store and read how an operation that failed can be retried, and whether it may be.
/// </summary>
public static class ErrorResilienceExtensions
{
    /// <summary>
    ///     The members that read the retry hint of an <see cref="Error" /> and whether it may be retried, and store them on a copy.
    /// </summary>
    /// <param name="error">The error.</param>
    extension(Error error)
    {
        /// <summary>
        ///     Tells whether the error says that the operation may be retried.
        /// </summary>
        /// <param name="timeProvider">
        ///     The clock for a retry hint with a date without an offset, which is readable only while it is in the future (passed on to
        ///     <see cref="TryGetRetryAfter" />); <see langword="null" /> means <see cref="TimeProvider.System" />.
        /// </param>
        /// <returns><see langword="true" /> if the error may be retried; otherwise <see langword="false" />.</returns>
        /// <remarks>
        ///     <para>The first rule that matches gives the answer:</para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 <b>The flag.</b> The value of <see cref="ResilienceMetadataKeys.Retryable" /> is a <see cref="bool" />: that value.
        ///                 <see langword="true" /> makes any error retryable; <see langword="false" /> forbids a retry, also with a retry hint. A value
        ///                 that is not a <see cref="bool" /> counts as absent.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <b>The hint.</b> The retry hint is readable (<see cref="TryGetRetryAfter" />): <see langword="true" />, whatever the category
        ///                 (e.g., a <see cref="ErrorCategory.Validation" /> error of a <c>413</c> response with <c>Retry-After</c>).
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <b>The category.</b> <see cref="ErrorCategory.Unavailable" />, <see cref="ErrorCategory.Timeout" /> or
        ///                 <see cref="ErrorCategory.LimitExceeded" />: <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Otherwise <see langword="false" />, also for <see cref="ErrorCategory.Critical" /> and
        ///                 <see cref="ErrorCategory.NotImplemented" />.
        ///             </description>
        ///         </item>
        ///     </list>
        ///     <para>
        ///         The answer describes the <b>error</b>. Whether the operation is safe to repeat (idempotent, RFC 9110, section 9.2.2) is for the
        ///         caller to know: e.g., a payment that timed out may have been made.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        public bool IsRetryable(TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(error);

            if (error.TryGetMetadata<bool>(ResilienceMetadataKeys.Retryable, out var retryable))
                return retryable;

            if (error.TryGetRetryAfter(out _, timeProvider))
                return true;

            return error.Category is ErrorCategory.Unavailable or ErrorCategory.Timeout or ErrorCategory.LimitExceeded;
        }

        /// <summary>
        ///     Reads the retry hint of the error.
        /// </summary>
        /// <param name="retryAfter">The hint, if the result is <see langword="true" />; otherwise <see langword="null" />.</param>
        /// <param name="timeProvider">
        ///     The clock for a date without an offset, which is readable only while it is in the future (<see cref="TimeProvider.GetUtcNow" />);
        ///     <see langword="null" /> means <see cref="TimeProvider.System" />. No other rule of the reader needs a clock.
        /// </param>
        /// <returns><see langword="true" /> if the error has a readable hint; otherwise <see langword="false" />.</returns>
        /// <remarks>
        ///     <para>
        ///         Both keys are read and combined: <see cref="RetryAfter.Delay" /> is the readable value
        ///         of <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />, and <see cref="RetryAfter.RetryAt" /> the readable value
        ///         of <see cref="ResilienceMetadataKeys.RetryAfterDate" />. When both are readable, the date says when the wait ends and the seconds say
        ///         what the source gave. When neither is, the result is <see langword="false" />.
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />: a whole number of seconds that is not negative and fits
        ///                 a <see cref="TimeSpan" />, of any integral type (e.g., <see cref="int" /> or <see cref="long" />), or a <see cref="decimal" />,
        ///                 <see cref="double" /> or <see cref="float" /> without a fraction (a deserializer may return any of them for a JSON <c>120</c>).
        ///                 Not readable: a fraction, a negative value, NaN, an infinity, a value too large, a string, <see langword="null" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <see cref="ResilienceMetadataKeys.RetryAfterDate" />: an RFC 3339 text with an upper-case <c>T</c>, any fraction of a second, and
        ///                 <c>Z</c>, a numeric offset or no offset (e.g., <c>2026-10-21T07:28:00+00:00</c>); an HTTP-date in the preferred format of RFC 9110
        ///                 (e.g., <c>Wed, 21 Oct 2026 07:28:00 GMT</c>); a <see cref="DateTimeOffset" />; or a <see cref="DateTime" />.
        ///                 It is returned in UTC. Not readable: any other text, a number, <see langword="null" />.
        ///             </description>
        ///         </item>
        ///     </list>
        ///     <para>
        ///         A date that names its offset (<c>Z</c>, <c>GMT</c>, a numeric offset, a <see cref="DateTimeOffset" />, a <see cref="DateTime" /> of kind
        ///         <see cref="DateTimeKind.Utc" /> or <see cref="DateTimeKind.Local" />) is readable also when it has passed: the wait is over, and
        ///         <see cref="RetryAfter.GetRemaining" /> gives <see cref="TimeSpan.Zero" />. A date without an offset (text without one, or
        ///         a <see cref="DateTime" /> of kind <see cref="DateTimeKind.Unspecified" />) is assumed to be UTC, and it is readable only while that
        ///         instant is in the future.
        ///     </para>
        ///     <para>
        ///         The keys are looked up with their exact spelling. No stored value makes this method throw: the values can come from another service.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        public bool TryGetRetryAfter([NotNullWhen(true)] out RetryAfter? retryAfter, TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(error);

            var metadata = error.Metadata;
            var delay = ReadDelay(metadata);
            var retryAt = ReadRetryAt(metadata, timeProvider);

            if (delay is null && retryAt is null)
            {
                retryAfter = null;

                return false;
            }

            retryAfter = new RetryAfter(delay, retryAt);

            return true;
        }

        /// <summary>
        ///     Returns a copy of the error that stores the delay before the operation is retried;
        ///     the rules are those of <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan)" />.
        /// </summary>
        /// <param name="delay">The delay; zero or more. A fraction of a second is rounded up to the next whole second.</param>
        /// <returns>A new <see cref="Error" /> with the hint.</returns>
        /// <remarks>
        ///     This stores a delay whose start is not known; where a delay is received, use <see cref="WithRetryAfter(Error, TimeSpan, DateTimeOffset)" />.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative.</exception>
        public Error WithRetryAfter(TimeSpan delay)
        {
            ArgumentNullException.ThrowIfNull(error);

            var builder = error.ToBuilder();
            builder.WithRetryAfter(delay);

            return builder.Build();
        }

        /// <summary>
        ///     Returns a copy of the error that stores the earliest time to retry the operation;
        ///     the rules are those of <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, DateTimeOffset)" />.
        /// </summary>
        /// <param name="retryAt">The time; it may be in the past and have any offset. It is stored in UTC, rounded up to the next whole second.</param>
        /// <returns>A new <see cref="Error" /> with the hint.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        public Error WithRetryAfter(DateTimeOffset retryAt)
        {
            ArgumentNullException.ThrowIfNull(error);

            var builder = error.ToBuilder();
            builder.WithRetryAfter(retryAt);

            return builder.Build();
        }

        /// <summary>
        ///     Returns a copy of the error that stores a delay that was received, with the time it was received;
        ///     the rules are those of <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)" />.
        /// </summary>
        /// <param name="delay">The delay as it was received; zero or more. A fraction of a second is rounded up to the next whole second.</param>
        /// <param name="receivedAt">When the delay was received; it may be in the past and have any offset.</param>
        /// <returns>A new <see cref="Error" /> with the hint: the delay, and when the wait ends.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative.</exception>
        public Error WithRetryAfter(TimeSpan delay, DateTimeOffset receivedAt)
        {
            ArgumentNullException.ThrowIfNull(error);

            var builder = error.ToBuilder();
            builder.WithRetryAfter(delay, receivedAt);

            return builder.Build();
        }

        /// <summary>
        ///     Returns a copy of the error that stores whether the operation may be retried; the rules are those of
        ///     <see cref="ErrorBuilderResilienceExtensions.WithRetryable" />.
        /// </summary>
        /// <param name="retryable">
        ///     <see langword="true" /> makes the error retryable; <see langword="false" /> forbids a retry, also with a retry hint.
        /// </param>
        /// <returns>A new <see cref="Error" /> with the flag.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
        public Error WithRetryable(bool retryable)
        {
            ArgumentNullException.ThrowIfNull(error);

            var builder = error.ToBuilder();
            builder.WithRetryable(retryable);

            return builder.Build();
        }
    }

    #region helper methods

    private static TimeSpan? ReadDelay(IReadOnlyDictionary<string, object?> metadata)
    {
        if (!metadata.TryGetValue(ResilienceMetadataKeys.RetryAfterSeconds, out var value)
            || !RetryAfterReader.TryReadSeconds(value, out var delay))
        {
            return null;
        }

        return delay;
    }

    /// <summary>
    ///     Reads the date, in UTC. A date without an offset is trusted only when the assumed UTC instant is in the future.
    /// </summary>
    private static DateTimeOffset? ReadRetryAt(IReadOnlyDictionary<string, object?> metadata, TimeProvider? timeProvider)
    {
        if (!metadata.TryGetValue(ResilienceMetadataKeys.RetryAfterDate, out var value)
            || !RetryAfterReader.TryReadDate(value, out var instant, out var hasOffset))
        {
            return null;
        }

        var utc = instant.ToUniversalTime();
        if (hasOffset)
            return utc;

        var clock = timeProvider ?? TimeProvider.System;
        var isInFuture = utc > clock.GetUtcNow();

        return isInFuture
            ? utc
            : null;
    }

    #endregion
}
