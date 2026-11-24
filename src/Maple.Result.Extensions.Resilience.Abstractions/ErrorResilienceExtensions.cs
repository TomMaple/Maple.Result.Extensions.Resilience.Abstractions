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

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     Extension methods of <see cref="Error" /> that store and read how an operation that failed can be retried.
/// </summary>
public static class ErrorResilienceExtensions
{
    /// <summary>
    ///     Reads the retry hint of the error: the delay before the operation is retried.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="delay">
    ///     The delay, if the result is <see langword="true" />: zero or more; <see cref="TimeSpan.Zero" /> for a date that has passed. Otherwise
    ///     <see cref="TimeSpan.Zero" />.
    /// </param>
    /// <param name="timeProvider">
    ///     The clock that gives “now” for a date (<see cref="TimeProvider.GetUtcNow" />); <see langword="null" /> means <see cref="TimeProvider.System" />.
    /// </param>
    /// <returns><see langword="true" /> if the error has a readable hint; otherwise <see langword="false" />.</returns>
    /// <remarks>
    ///     <para>The first readable hint wins, the seconds before the date:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />: a whole number of seconds that is not negative and fits a
    ///                 <see cref="TimeSpan" />, of any integral type (e.g., <see cref="int" /> or <see cref="long" />), or a <see cref="decimal" />,
    ///                 <see cref="double" /> or <see cref="float" /> without a fraction (a deserializer may return any of them for a JSON <c>120</c>).
    ///                 Not readable: a fraction, a negative value, NaN, an infinity, a value too large, a string, <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResilienceMetadataKeys.RetryAfterDate" />: an RFC 3339 text with an upper-case <c>T</c>, any fraction of a second, and
    ///                 <c>Z</c>, a numeric offset or no offset (e.g., <c>2026-10-21T07:28:00+00:00</c>); an HTTP-date in the preferred format of RFC 9110
    ///                 (e.g., <c>Wed, 21 Oct 2026 07:28:00 GMT</c>); a <see cref="DateTimeOffset" />; or a <see cref="DateTime" />. The delay is the time
    ///                 from now until that instant. Not readable: any other text, a number, <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         A date that names its offset (<c>Z</c>, <c>GMT</c>, a numeric offset, a <see cref="DateTimeOffset" />, a <see cref="DateTime" /> of kind
    ///         <see cref="DateTimeKind.Utc" /> or <see cref="DateTimeKind.Local" />) and has passed gives <see cref="TimeSpan.Zero" />: the wait is over.
    ///         A date without an offset (text without one, or a <see cref="DateTime" /> of kind <see cref="DateTimeKind.Unspecified" />) is assumed to
    ///         be UTC, and it is readable only while that instant is in the future.
    ///     </para>
    ///     <para>
    ///         The keys are looked up with their exact spelling. No stored value makes this method throw: the values can come from another service.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
    public static bool TryGetRetryAfter(this Error error, out TimeSpan delay, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        var metadata = error.Metadata;
        if (metadata.TryGetValue(ResilienceMetadataKeys.RetryAfterSeconds, out var seconds) && RetryAfterReader.TryReadSeconds(seconds, out delay))
            return true;

        if (metadata.TryGetValue(ResilienceMetadataKeys.RetryAfterDate, out var date)
            && RetryAfterReader.TryReadDate(date, out var instant, out var hasOffset))
        {
            var clock = timeProvider ?? TimeProvider.System;
            var remaining = instant - clock.GetUtcNow();
            if (remaining > TimeSpan.Zero)
            {
                delay = remaining;

                return true;
            }

            // A date without an offset is trusted only when the assumed UTC instant is in the future.
            if (hasOffset)
            {
                delay = TimeSpan.Zero;

                return true;
            }
        }

        delay = TimeSpan.Zero;

        return false;
    }

    /// <summary>
    ///     Returns a copy of the error that stores the delay before the operation is retried; the rules are those of
    ///     <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan)" />.
    /// </summary>
    /// <param name="error">The error; it is not changed.</param>
    /// <param name="delay">The delay; zero or more. A fraction of a second is rounded up to the next whole second.</param>
    /// <returns>A new <see cref="Error" /> with the hint.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative.</exception>
    public static Error WithRetryAfter(this Error error, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(error);

        var builder = error.ToBuilder();
        builder.WithRetryAfter(delay);

        return builder.Build();
    }

    /// <summary>
    ///     Returns a copy of the error that stores the earliest time to retry the operation; the rules are those of
    ///     <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, DateTimeOffset)" />.
    /// </summary>
    /// <param name="error">The error; it is not changed.</param>
    /// <param name="retryAt">The time; it may be in the past and have any offset. It is stored in UTC, rounded up to the next whole second.</param>
    /// <returns>A new <see cref="Error" /> with the hint.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="error" /> is <see langword="null" />.</exception>
    public static Error WithRetryAfter(this Error error, DateTimeOffset retryAt)
    {
        ArgumentNullException.ThrowIfNull(error);

        var builder = error.ToBuilder();
        builder.WithRetryAfter(retryAt);

        return builder.Build();
    }
}
