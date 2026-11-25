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
using System.Globalization;
using System.Linq;

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     Extension methods of <see cref="ErrorBuilder" /> that store how an operation that failed can be retried, and whether it may be.
/// </summary>
/// <remarks>
///     <list type="table">
///         <listheader>
///             <term>Written with</term>
///             <description>Stored</description>
///         </listheader>
///         <item>
///             <term>
///                 <see cref="WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)" />
///             </term>
///             <description>
///                 <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> (the delay) and <see cref="ResilienceMetadataKeys.RetryAfterDate" />
///                 (when the wait ends): a delay that was received, e.g., in a response.
///             </description>
///         </item>
///         <item>
///             <term>
///                 <see cref="WithRetryAfter(ErrorBuilder, DateTimeOffset)" />
///             </term>
///             <description><see cref="ResilienceMetadataKeys.RetryAfterDate" /> only: a date.</description>
///         </item>
///         <item>
///             <term>
///                 <see cref="WithRetryAfter(ErrorBuilder, TimeSpan)" />
///             </term>
///             <description><see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> only: a delay whose start is not known.</description>
///         </item>
///     </list>
/// </remarks>
public static class ErrorBuilderResilienceExtensions
{
    #region consts

    private const string RetryAfterDateFormat = "yyyy-MM-dd'T'HH:mm:sszzz";

    #endregion

    /// <summary>
    ///     The members that store the retry hint and the <see cref="ResilienceMetadataKeys.Retryable" /> flag on an <see cref="ErrorBuilder" />.
    /// </summary>
    /// <param name="builder">The builder of the error.</param>
    extension(ErrorBuilder builder)
    {
        /// <summary>
        ///     Stores the delay before the operation is retried, in whole seconds, under <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />,
        ///     and removes the date of <see cref="ResilienceMetadataKeys.RetryAfterDate" />.
        /// </summary>
        /// <param name="delay">
        ///     The delay; zero or more. A fraction of a second is rounded up to the next whole second, so the hint is never earlier than asked
        ///     (e.g., 1.2 seconds are stored as <c>2L</c>); at most <c>922337203685L</c> is stored, the largest number of whole seconds of
        ///     a <see cref="TimeSpan" />.
        /// </param>
        /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
        /// <remarks>
        ///     <para>
        ///         This stores a delay whose start is not known, so a reader counts it from the moment it reads; it is the right overload for an error
        ///         that is created once and used many times (e.g., a static field). Where a delay is received (e.g., in a response),
        ///         use <see cref="WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)" />, which also stores when the wait ends.
        ///     </para>
        ///     <para>
        ///         The value is a <see cref="long" />. Every existing key that equals <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> or
        ///         <see cref="ResilienceMetadataKeys.RetryAfterDate" /> ignoring case (e.g., <c>RetryAfterSeconds</c> taken from a response body)
        ///         is removed first, so the hint replaces it.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative; the builder is unchanged.</exception>
        public ErrorBuilder WithRetryAfter(TimeSpan delay)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

            var seconds = WholeSeconds.RoundUp(delay);

            RemoveKeysIgnoringCase(builder, ResilienceMetadataKeys.RetryAfterSeconds, ResilienceMetadataKeys.RetryAfterDate);

            return builder.WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, seconds);
        }

        /// <summary>
        ///     Stores the earliest time to retry the operation under <see cref="ResilienceMetadataKeys.RetryAfterDate" />, and removes
        ///     the delay of <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />.
        /// </summary>
        /// <param name="retryAt">
        ///     The time; it may be in the past and have any offset. It is converted to UTC, and a fraction of a second is rounded up to the next whole second,
        ///     so the hint is never earlier than asked;
        ///     only a time in the last second before <see cref="DateTimeOffset.MaxValue" /> loses its fraction instead.
        /// </param>
        /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
        /// <remarks>
        ///     The value is a <see cref="string" /> in the format <c>yyyy-MM-dd'T'HH:mm:sszzz</c> (invariant culture) with the offset <c>+00:00</c>, e.g.,
        ///     <c>2026-10-21T07:28:00+00:00</c>. Every existing key that equals <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> or
        ///     <see cref="ResilienceMetadataKeys.RetryAfterDate" /> ignoring case (e.g., <c>RetryAfterDate</c> taken from a response body) is removed first,
        ///     so the hint replaces it.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
        public ErrorBuilder WithRetryAfter(DateTimeOffset retryAt)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var text = FormatRetryAfterDate(retryAt);

            RemoveKeysIgnoringCase(builder, ResilienceMetadataKeys.RetryAfterSeconds, ResilienceMetadataKeys.RetryAfterDate);

            return builder.WithMetadata(ResilienceMetadataKeys.RetryAfterDate, text);
        }

        /// <summary>
        ///     Stores a delay that was received, with the time it was received: the delay under <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />, and
        ///     when the wait ends under <see cref="ResilienceMetadataKeys.RetryAfterDate" />.
        /// </summary>
        /// <param name="delay">
        ///     The delay as it was received (e.g., <c>Retry-After: 120</c>); zero or more. A fraction of a second is rounded up to the next whole second,
        ///     and at most <c>922337203685L</c> is stored, as by <see cref="WithRetryAfter(ErrorBuilder, TimeSpan)" />.
        /// </param>
        /// <param name="receivedAt">
        ///     When the delay was received (e.g., the time of the response); it may be in the past and have any offset. A delay counts from this moment
        ///     (RFC 9110, section 10.2.3).
        /// </param>
        /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
        /// <remarks>
        ///     <para>
        ///         The seconds are stored as <see cref="WithRetryAfter(ErrorBuilder, TimeSpan)" /> stores them,
        ///         and the date as <see cref="WithRetryAfter(ErrorBuilder, DateTimeOffset)" /> stores it:
        ///         <paramref name="receivedAt" /> plus those whole seconds.
        ///         A reader that comes later gets the time that is really left, not the full delay again; the delay as it was received stays.
        ///     </para>
        ///     <para>
        ///         Where a delay is received (a response, a rejection), this is the right overload. It is wrong for an error that is reused
        ///         (e.g., a static field): its instant passes; use <see cref="WithRetryAfter(ErrorBuilder, TimeSpan)" /> there.
        ///     </para>
        ///     <para>
        ///         When the sum is beyond <see cref="DateTimeOffset.MaxValue" />, only the seconds are stored: there is no instant to store, and this method
        ///         does not throw for it, because it runs while a failure is handled. Every existing key that equals either key ignoring case is removed first.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative; the builder is unchanged.</exception>
        public ErrorBuilder WithRetryAfter(TimeSpan delay, DateTimeOffset receivedAt)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

            var seconds = WholeSeconds.RoundUp(delay);

            RemoveKeysIgnoringCase(builder, ResilienceMetadataKeys.RetryAfterSeconds, ResilienceMetadataKeys.RetryAfterDate);
            builder.WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, seconds);

            // Compared in whole seconds, so that the sum is calculated only when it is in range.
            var secondsToMaxValue = (DateTimeOffset.MaxValue.UtcTicks - receivedAt.UtcTicks) / TimeSpan.TicksPerSecond;
            if (seconds > secondsToMaxValue)
                return builder;

            // In UTC: with a positive offset, the clock time of receivedAt could pass DateTimeOffset.MaxValue before the instant does.
            var receivedAtUtc = receivedAt.ToUniversalTime();
            var retryAt = receivedAtUtc.AddTicks(seconds * TimeSpan.TicksPerSecond);
            var text = FormatRetryAfterDate(retryAt);

            return builder.WithMetadata(ResilienceMetadataKeys.RetryAfterDate, text);
        }

        /// <summary>
        ///     Stores under <see cref="ResilienceMetadataKeys.Retryable" /> whether the operation may be retried, whatever the category and the retry
        ///     hint say.
        /// </summary>
        /// <param name="retryable">
        ///     <see langword="true" /> makes the error retryable (e.g., a <see cref="ErrorCategory.Validation" /> error of a service that is being
        ///     deployed); <see langword="false" /> forbids a retry, also for an <see cref="ErrorCategory.Unavailable" /> error and with a retry hint
        ///     (e.g., a monthly quota that is used up).
        /// </param>
        /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
        /// <remarks>
        ///     The value is a <see cref="bool" />. Every existing key that equals <see cref="ResilienceMetadataKeys.Retryable" /> ignoring case
        ///     (e.g., <c>Retryable</c> taken from a response body) is removed first, so the flag replaces it. The retry hint is not changed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
        public ErrorBuilder WithRetryable(bool retryable)
        {
            ArgumentNullException.ThrowIfNull(builder);

            RemoveKeysIgnoringCase(builder, ResilienceMetadataKeys.Retryable);

            return builder.WithMetadata(ResilienceMetadataKeys.Retryable, retryable);
        }
    }

    #region helper methods

    /// <summary>
    ///     Formats a time as the value of <see cref="ResilienceMetadataKeys.RetryAfterDate" />: in UTC, a fraction of a second rounded up.
    /// </summary>
    private static string FormatRetryAfterDate(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        var rounded = WholeSeconds.RoundUp(utc);

        return rounded.ToString(RetryAfterDateFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Removes the keys and every key that differs from them only in case: <see cref="ErrorBuilder.WithMetadata(string, object)" /> throws for a
    ///     key that differs only in case from an existing one.
    /// </summary>
    /// <remarks>
    ///     The builder does not expose its metadata, so the keys are read from a snapshot (<see cref="ErrorBuilder.Build" /> never throws), and
    ///     <see cref="ErrorBuilder.WithoutMetadata(string)" /> removes each with its exact spelling.
    /// </remarks>
    private static void RemoveKeysIgnoringCase(ErrorBuilder builder, params string[] keys)
    {
        var snapshot = builder.Build();
        var matchingKeys = snapshot.Metadata.Keys.Where(key => keys.Contains(key, StringComparer.OrdinalIgnoreCase));
        foreach (var key in matchingKeys)
            builder.WithoutMetadata(key);
    }

    #endregion
}
