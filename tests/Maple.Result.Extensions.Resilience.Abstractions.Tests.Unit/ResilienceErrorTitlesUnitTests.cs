// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit;

/// <summary>
/// The titles go with the type URIs of <see cref="ResilienceErrorUris" />: a client may show them, so a change is visible.
/// </summary>
public class ResilienceErrorTitlesUnitTests
{
    #region titles

    [Theory]
    [InlineData(nameof(ResilienceErrorTitles.Timeout), ResilienceErrorTitles.Timeout, "The operation timed out.")]
    [InlineData(nameof(ResilienceErrorTitles.CircuitOpen), ResilienceErrorTitles.CircuitOpen, "The circuit breaker is open.")]
    [InlineData(nameof(ResilienceErrorTitles.RateLimited), ResilienceErrorTitles.RateLimited, "The rate limit was exceeded.")]
    public void Title_Member_HasExactText(string constant, string title, string expected)
    {
        // Act & Assert
        title.ShouldBe(expected, constant);
    }

    #endregion
}
