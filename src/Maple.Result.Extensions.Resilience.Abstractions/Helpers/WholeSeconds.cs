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

namespace Maple.Results.Extensions.Resilience.Abstractions.Helpers;

/// <summary>
///     Rounds durations and instants up to whole seconds: a retry hint is never earlier than asked.
/// </summary>
internal static class WholeSeconds
{
    #region consts

    /// <summary>
    ///     The largest number of whole seconds that a <see cref="TimeSpan" /> holds: 922,337,203,685.
    /// </summary>
    public const long MaxSeconds = long.MaxValue / TimeSpan.TicksPerSecond;

    #endregion

    /// <summary>
    ///     Returns the number of whole seconds of a duration that is not negative, a fraction rounded up, and at most <see cref="MaxSeconds" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Integer arithmetic: <see cref="TimeSpan.TotalSeconds" /> is a <see cref="double" />, which cannot hold every tick of a long duration.
    ///     </para>
    ///     <para>
    ///         The cap: rounded up, a duration in the last second before <see cref="TimeSpan.MaxValue" /> would be one second more than
    ///         a <see cref="TimeSpan" /> holds, and the reader would not accept the stored value.
    ///     </para>
    /// </remarks>
    public static long RoundUp(TimeSpan duration)
    {
        var seconds = Math.DivRem(duration.Ticks, TimeSpan.TicksPerSecond, out var fractionTicks);

        return fractionTicks == 0
            ? seconds
            : Math.Min(seconds + 1, MaxSeconds);
    }

    /// <summary>
    ///     Rounds a fraction of a second up to the next whole second; in the last second before <see cref="DateTimeOffset.MaxValue" />,
    ///     where that would overflow, the fraction is dropped.
    /// </summary>
    public static DateTimeOffset RoundUp(DateTimeOffset value)
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
