// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit.TestingInfrastructure.Fakes;
using System;

namespace Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit;

public class RetryAfterUnitTests
{
    private const string Title = "The service is unavailable.";
    private static readonly ErrorUri TypeUri = ErrorUri.Tag("tag:test.com,2024:Test");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 7, 0, 0, TimeSpan.Zero);
    private static readonly TimeProvider Clock = new FixedTimeProvider(Now);

    #region ctor

    [Fact]
    public void Ctor_NeitherDelayNorRetryAt_ThrowsArgumentException()
    {
        // Act
        var exception = Record.Exception(() => new RetryAfter(null, null));

        // Assert
        exception.ShouldBeOfType<ArgumentException>();
    }

    [Fact]
    public void Ctor_NegativeDelay_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var exception = Record.Exception(() => new RetryAfter(TimeSpan.FromTicks(-1), null));

        // Assert
        exception.ShouldBeOfType<ArgumentOutOfRangeException>().ParamName.ShouldBe("delay");
    }

    #endregion

    #region Kind

    [Fact]
    public void Kind_DelayOnly_IsDelay()
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.FromSeconds(120), null);

        // Act
        var kind = retryAfter.Kind;

        // Assert
        kind.ShouldBe(RetryAfterKind.Delay);
    }

    [Fact]
    public void Kind_RetryAtOnly_IsDate()
    {
        // Arrange
        var retryAfter = new RetryAfter(null, Now.AddMinutes(28));

        // Act
        var kind = retryAfter.Kind;

        // Assert
        kind.ShouldBe(RetryAfterKind.Date);
    }

    [Fact]
    public void Kind_DelayAndRetryAt_IsDelay()
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.FromSeconds(120), Now.AddSeconds(120));

        // Act
        var kind = retryAfter.Kind;

        // Assert
        kind.ShouldBe(RetryAfterKind.Delay);
    }

    #endregion

    #region GetRemaining

    [Fact]
    public void GetRemaining_RetryAtInFuture_ReturnsTimeUntilRetryAt()
    {
        // Arrange
        var retryAfter = new RetryAfter(null, Now.AddMinutes(28));

        // Act
        var remaining = retryAfter.GetRemaining(Clock);

        // Assert
        remaining.ShouldBe(TimeSpan.FromMinutes(28));
    }

    [Fact]
    public void GetRemaining_RetryAtEqualToNow_ReturnsZero()
    {
        // Arrange
        var retryAfter = new RetryAfter(null, Now);

        // Act
        var remaining = retryAfter.GetRemaining(Clock);

        // Assert
        remaining.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void GetRemaining_RetryAtInPast_ReturnsZero()
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.FromSeconds(30), Now.AddMinutes(-5));

        // Act
        var remaining = retryAfter.GetRemaining(Clock);

        // Assert
        remaining.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(2000)]
    [InlineData(2026)]
    [InlineData(9000)]
    public void GetRemaining_DelayOnly_ReturnsDelayWhateverClock(int year)
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.FromSeconds(120), null);
        var clock = new FixedTimeProvider(new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // Act
        var remaining = retryAfter.GetRemaining(clock);

        // Assert
        remaining.ShouldBe(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void GetRemaining_DelayAndRetryAt_CountsFromRetryAt()
    {
        // Arrange (a delay of 30 s, received 20 s ago)
        var error = Error.Unavailable(TypeUri, Title)
            .WithRetryAfter(TimeSpan.FromSeconds(30), Now.AddSeconds(-20));
        error.TryGetRetryAfter(out var retryAfter, Clock).ShouldBeTrue();

        // Act
        var remaining = retryAfter.GetRemaining(Clock);

        // Assert
        remaining.ShouldBe(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GetRemaining_NoTimeProvider_UsesSystemClock()
    {
        // Arrange
        var retryAfter = new RetryAfter(null, DateTimeOffset.UtcNow.AddYears(1));

        // Act
        var remaining = retryAfter.GetRemaining();

        // Assert
        remaining.ShouldBeGreaterThan(TimeSpan.FromDays(300));
    }

    #endregion

    #region GetRemainingSeconds

    [Theory]
    [InlineData(10_000, 10L)]
    [InlineData(9_200, 10L)]
    [InlineData(9_999, 10L)]
    [InlineData(1, 1L)]
    [InlineData(0, 0L)]
    [InlineData(-5_000, 0L)]
    public void GetRemainingSeconds_RetryAt_ReturnsWholeSecondsRoundedUp(int millisecondsFromNow, long expectedSeconds)
    {
        // Arrange
        var retryAfter = new RetryAfter(null, Now.AddMilliseconds(millisecondsFromNow));

        // Act
        var seconds = retryAfter.GetRemainingSeconds(Clock);

        // Assert
        seconds.ShouldBe(expectedSeconds);
    }

    [Fact]
    public void GetRemainingSeconds_DelayWithFraction_RoundsUp()
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.FromMilliseconds(9_200), null);

        // Act
        var seconds = retryAfter.GetRemainingSeconds(Clock);

        // Assert
        seconds.ShouldBe(10L);
    }

    [Fact]
    public void GetRemainingSeconds_MaxDelay_DoesNotOverflow()
    {
        // Arrange
        var retryAfter = new RetryAfter(TimeSpan.MaxValue, null);

        // Act
        var seconds = retryAfter.GetRemainingSeconds(Clock);

        // Assert
        seconds.ShouldBe(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerSecond + 1);
    }

    [Fact]
    public void GetRemainingSeconds_NoTimeProvider_UsesSystemClock()
    {
        // Arrange
        var retryAfter = new RetryAfter(null, DateTimeOffset.UtcNow.AddYears(1));

        // Act
        var seconds = retryAfter.GetRemainingSeconds();

        // Assert
        seconds.ShouldBeGreaterThan(300L * 24 * 60 * 60);
    }

    #endregion

    #region equality

    [Fact]
    public void Equals_SameDelayAndRetryAt_IsTrue()
    {
        // Arrange
        var first = new RetryAfter(TimeSpan.FromSeconds(120), Now.AddSeconds(120));
        var second = new RetryAfter(TimeSpan.FromSeconds(120), Now.AddSeconds(120));

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void Equals_SameInstantWithOtherOffset_IsTrue()
    {
        // Arrange
        var first = new RetryAfter(null, new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));
        var second = new RetryAfter(null, new DateTimeOffset(2026, 10, 21, 9, 28, 0, TimeSpan.FromHours(2)));

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void Equals_OtherDelay_IsFalse()
    {
        // Arrange
        var first = new RetryAfter(TimeSpan.FromSeconds(120), Now.AddSeconds(120));
        var second = new RetryAfter(TimeSpan.FromSeconds(60), Now.AddSeconds(120));

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_OtherRetryAt_IsFalse()
    {
        // Arrange
        var first = new RetryAfter(TimeSpan.FromSeconds(120), Now.AddSeconds(120));
        var second = new RetryAfter(TimeSpan.FromSeconds(120), null);

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.ShouldBeFalse();
    }

    #endregion
}
