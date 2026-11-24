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
///     The form in which the source gave a retry hint (<see cref="RetryAfter.Kind" />).
/// </summary>
public enum RetryAfterKind
{
    /// <summary>
    ///     The source gave a number of seconds, e.g., <c>Retry-After: 120</c>; <see cref="RetryAfter.Delay" /> has a value.
    /// </summary>
    Delay,

    /// <summary>
    ///     The source gave a date, e.g., <c>Retry-After: Wed, 21 Oct 2026 07:28:00 GMT</c>; <see cref="RetryAfter.Delay" /> is <see langword="null" /> and
    ///     <see cref="RetryAfter.RetryAt" /> has a value.
    /// </summary>
    Date,
}
