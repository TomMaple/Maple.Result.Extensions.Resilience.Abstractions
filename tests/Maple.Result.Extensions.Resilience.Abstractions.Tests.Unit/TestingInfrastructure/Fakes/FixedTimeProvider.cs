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

namespace Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit.TestingInfrastructure.Fakes;

/// <summary>
/// A clock that always returns the same time, so that a delay computed from a date is exact.
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
        => utcNow;
}
