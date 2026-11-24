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
using System.Globalization;
using System.Linq;

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     Extension methods of <see cref="ErrorBuilder" /> that store how an operation that failed can be retried.
/// </summary>
public static class ErrorBuilderResilienceExtensions
{
    private const string RetryAfterDateFormat = "yyyy-MM-dd'T'HH:mm:sszzz";

    /// <summary>
    ///     Stores the delay before the operation is retried, in whole seconds, under <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />, and removes
    ///     the date of <see cref="ResilienceMetadataKeys.RetryAfterDate" />.
    /// </summary>
    /// <param name="builder">The builder of the error.</param>
    /// <param name="delay">
    ///     The delay; zero or more. A fraction of a second is rounded up to the next whole second, so the hint is never earlier than asked
    ///     (e.g., 1.2 seconds are stored as <c>2L</c>).
    /// </param>
    /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
    /// <remarks>
    ///     The value is a <see cref="long" />. Every existing key that equals <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> or
    ///     <see cref="ResilienceMetadataKeys.RetryAfterDate" /> ignoring case (e.g., <c>RetryAfterSeconds</c> taken from a response body) is removed first,
    ///     so the hint replaces it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative; the builder is unchanged.</exception>
    public static ErrorBuilder WithRetryAfter(this ErrorBuilder builder, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

        // Integer arithmetic: TotalSeconds is a double, which cannot hold every tick of a large delay.
        var seconds = Math.DivRem(delay.Ticks, TimeSpan.TicksPerSecond, out var fractionTicks);
        if (fractionTicks > 0)
            seconds++;

        RemoveRetryAfterKeys(builder);

        return builder.WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, seconds);
    }

    /// <summary>
    ///     Stores the earliest time to retry the operation under <see cref="ResilienceMetadataKeys.RetryAfterDate" />, and removes the delay of
    ///     <see cref="ResilienceMetadataKeys.RetryAfterSeconds" />.
    /// </summary>
    /// <param name="builder">The builder of the error.</param>
    /// <param name="retryAt">
    ///     The time; it may be in the past and have any offset. It is converted to UTC, and a fraction of a second is rounded up to the next whole second,
    ///     so the hint is never earlier than asked; only a time in the last second before <see cref="DateTimeOffset.MaxValue" /> loses its fraction instead.
    /// </param>
    /// <returns>The same <see cref="ErrorBuilder" /> instance.</returns>
    /// <remarks>
    ///     The value is a <see cref="string" /> in the format <c>yyyy-MM-dd'T'HH:mm:sszzz</c> (invariant culture) with the offset <c>+00:00</c>, e.g.,
    ///     <c>2026-10-21T07:28:00+00:00</c>. Every existing key that equals <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> or
    ///     <see cref="ResilienceMetadataKeys.RetryAfterDate" /> ignoring case (e.g., <c>RetryAfterDate</c> taken from a response body) is removed first,
    ///     so the hint replaces it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">If the <paramref name="builder" /> is <see langword="null" />.</exception>
    public static ErrorBuilder WithRetryAfter(this ErrorBuilder builder, DateTimeOffset retryAt)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var utc = retryAt.ToUniversalTime();
        var rounded = RoundUpToWholeSecond(utc);
        var text = rounded.ToString(RetryAfterDateFormat, CultureInfo.InvariantCulture);

        RemoveRetryAfterKeys(builder);

        return builder.WithMetadata(ResilienceMetadataKeys.RetryAfterDate, text);
    }

    private static bool IsRetryAfterKey(string key)
        => string.Equals(key, ResilienceMetadataKeys.RetryAfterSeconds, StringComparison.OrdinalIgnoreCase)
           || string.Equals(key, ResilienceMetadataKeys.RetryAfterDate, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Removes both retry keys and every key that differs from them only in case: <see cref="ErrorBuilder.WithMetadata(string, object)" /> throws for a
    ///     key that differs only in case from an existing one.
    /// </summary>
    /// <remarks>
    ///     The builder does not expose its metadata, so the keys are read from a snapshot (<see cref="ErrorBuilder.Build" /> never throws), and
    ///     <see cref="ErrorBuilder.WithoutMetadata(string)" /> removes each with its exact spelling.
    /// </remarks>
    private static void RemoveRetryAfterKeys(ErrorBuilder builder)
    {
        var snapshot = builder.Build();
        var retryAfterKeys = snapshot.Metadata.Keys.Where(IsRetryAfterKey);
        foreach (var key in retryAfterKeys)
            builder.WithoutMetadata(key);
    }

    /// <summary>
    ///     Rounds a fraction of a second up to the next whole second; in the last second before <see cref="DateTimeOffset.MaxValue" />, where that would
    ///     overflow, the fraction is dropped.
    /// </summary>
    private static DateTimeOffset RoundUpToWholeSecond(DateTimeOffset value)
    {
        var fractionTicks = value.Ticks % TimeSpan.TicksPerSecond;
        if (fractionTicks == 0)
            return value;

        var ticksToNextSecond = TimeSpan.TicksPerSecond - fractionTicks;
        var canRoundUp = DateTimeOffset.MaxValue.Ticks - value.Ticks >= ticksToNextSecond;

        return canRoundUp
            ? value.AddTicks(ticksToNextSecond)
            : value.AddTicks(-fractionTicks);
    }
}
