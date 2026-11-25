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
///     The retry hint of an error: how long to wait before the operation is retried, as the source gave it, and when the wait ends.
/// </summary>
/// <remarks>
///     <para>
///         An instance comes from <see cref="ErrorResilienceExtensions.TryGetRetryAfter" /> or
///         <see cref="OperationResultResilienceExtensions.GetRetryAfter" />, and always holds a hint: <see cref="Delay" /> or <see cref="RetryAt" />
///         has a value, or both. An error without a hint gives <see langword="null" />.
///     </para>
///     <para>
///         Which member to use: a retry pipeline waits <see cref="GetRemaining" />; a package that writes a <c>Retry-After</c> header writes
///         <see cref="GetRemainingSeconds" />. Code that must not read the system clock (e.g., an orchestrator, which is replayed)
///         uses <see cref="RetryAt" /> as it is (e.g., as the time of a durable timer), or passes a <see cref="TimeProvider" /> of its own.
///     </para>
///     <para>Two instances are equal when their <see cref="Delay" /> and <see cref="RetryAt" /> are equal (<see cref="RetryAt" /> as an instant).</para>
/// </remarks>
public sealed record RetryAfter
{
    #region constructors

    /// <summary>
    ///     Creates a retry hint.
    /// </summary>
    /// <param name="delay">The delay as the source gave it; zero or more.</param>
    /// <param name="retryAt">When the wait ends.</param>
    /// <exception cref="ArgumentException">If both are <see langword="null" />: an instance always holds a hint.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="delay" /> is negative.</exception>
    internal RetryAfter(TimeSpan? delay, DateTimeOffset? retryAt)
    {
        if (delay is null && retryAt is null)
            throw new ArgumentException("A retry hint needs a delay, the time when the wait ends, or both.", nameof(delay));

        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "The delay of a retry hint must not be negative.");

        Delay = delay;
        RetryAt = retryAt;
    }

    #endregion

    /// <summary>
    ///     The delay as the source gave it (e.g., <c>Retry-After: 120</c>), zero or more; <see langword="null" /> if the source gave a date.
    /// </summary>
    /// <remarks>
    ///     It does not get shorter while time passes: use <see cref="GetRemaining" /> for the time that is left.
    /// </remarks>
    public TimeSpan? Delay { get; }

    /// <summary>
    ///     The form in which the source gave the hint: <see cref="RetryAfterKind.Delay" /> when <see cref="Delay" /> has a value, otherwise
    ///     <see cref="RetryAfterKind.Date" />.
    /// </summary>
    /// <remarks>
    ///     It is calculated from <see cref="Delay" />, so it cannot disagree with the values.
    /// </remarks>
    public RetryAfterKind Kind
        => Delay.HasValue
            ? RetryAfterKind.Delay
            : RetryAfterKind.Date;

    /// <summary>
    ///     When the wait ends, in UTC; <see langword="null" /> for a delay whose start is not known
    ///     (stored with <see cref="ErrorBuilderResilienceExtensions.WithRetryAfter(ErrorBuilder, TimeSpan)" />).
    /// </summary>
    /// <remarks>
    ///     It may be in the past: the wait is over. It does not depend on a clock, so it suits code that must not read the system clock.
    /// </remarks>
    public DateTimeOffset? RetryAt { get; }

    /// <summary>
    ///     Returns the time that is left until the operation can be retried.
    /// </summary>
    /// <param name="timeProvider">
    ///     The clock that gives “now” (<see cref="TimeProvider.GetUtcNow" />); <see langword="null" /> means <see cref="TimeProvider.System" />.
    /// </param>
    /// <returns>
    ///     With a <see cref="RetryAt" />: the time from now until then, and <see cref="TimeSpan.Zero" /> once it has passed.
    ///     Without one: <see cref="Delay" />, whatever the clock says.
    /// </returns>
    public TimeSpan GetRemaining(TimeProvider? timeProvider = null)
    {
        if (RetryAt is not { } retryAt)
            return Delay.GetValueOrDefault();

        var clock = timeProvider ?? TimeProvider.System;
        var remaining = retryAt - clock.GetUtcNow();

        return remaining > TimeSpan.Zero
            ? remaining
            : TimeSpan.Zero;
    }

    /// <summary>
    ///     Returns the time that is left in whole seconds, a fraction rounded up: the value for a <c>Retry-After</c> header.
    /// </summary>
    /// <param name="timeProvider">
    ///     The clock that gives “now” (<see cref="TimeProvider.GetUtcNow" />); <see langword="null" /> means <see cref="TimeProvider.System" />.
    /// </param>
    /// <returns>
    ///     <see cref="GetRemaining" /> in whole seconds, rounded up; zero or more, and at most <c>922337203685</c>,
    ///     the largest number of whole seconds of a <see cref="TimeSpan" />.
    /// </returns>
    public long GetRemainingSeconds(TimeProvider? timeProvider = null)
    {
        var remaining = GetRemaining(timeProvider);
        return WholeSeconds.RoundUp(remaining);
    }
}
