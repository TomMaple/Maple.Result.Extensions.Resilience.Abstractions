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
    /// <summary>
    ///     Returns the number of whole seconds of a duration that is not negative, a fraction rounded up.
    /// </summary>
    /// <remarks>
    ///     Integer arithmetic: <see cref="TimeSpan.TotalSeconds" /> is a <see cref="double" />, which cannot hold every tick of a long duration.
    /// </remarks>
    public static long RoundUp(TimeSpan duration)
    {
        var seconds = Math.DivRem(duration.Ticks, TimeSpan.TicksPerSecond, out var fractionTicks);

        return fractionTicks > 0
            ? seconds + 1
            : seconds;
    }

    /// <summary>
    ///     Rounds a fraction of a second up to the next whole second; in the last second before <see cref="DateTimeOffset.MaxValue" />, where that would
    ///     overflow, the fraction is dropped.
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
