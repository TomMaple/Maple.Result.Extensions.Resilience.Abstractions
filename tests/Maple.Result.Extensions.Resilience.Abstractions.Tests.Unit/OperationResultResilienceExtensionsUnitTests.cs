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

public class OperationResultResilienceExtensionsUnitTests
{
    private const string Title = "The service is unavailable.";
    private static readonly ErrorUri TypeUri = ErrorUri.Tag("tag:test.com,2024:Test");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 7, 0, 0, TimeSpan.Zero);
    private static readonly TimeProvider Clock = new FixedTimeProvider(Now);

    [Fact]
    public void GetRetryAfter_SuccessfulResult_ReturnsNull()
    {
        // Arrange
        IOperationResult result = Result.Success();

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_SuccessfulResultWithValue_ReturnsNull()
    {
        // Arrange
        IOperationResult result = Result.FromValue(42);

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithoutHint_ReturnsNull()
    {
        // Arrange
        IOperationResult result = Result.FromError(CreateError());

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithUnreadableHint_ReturnsNull()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, "120");

        IOperationResult result = Result.FromError(error);

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithSeconds_ReturnsDelay()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(TimeSpan.FromSeconds(120));

        IOperationResult result = Result<string>.FromError(error);

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldNotBeNull();
        retryAfter.Kind.ShouldBe(RetryAfterKind.Delay);
        retryAfter.Delay.ShouldBe(TimeSpan.FromSeconds(120));
        retryAfter.RetryAt.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithDate_ReturnsDate()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        IOperationResult result = Result.FromError(error);

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldNotBeNull();
        retryAfter.Kind.ShouldBe(RetryAfterKind.Date);
        retryAfter.RetryAt.ShouldBe(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));
        retryAfter.GetRemaining(Clock).ShouldBe(TimeSpan.FromMinutes(28));
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithReceivedDelay_ReturnsBoth()
    {
        // Arrange (a delay of 30 s, received 20 s ago)
        var error = CreateError()
            .WithRetryAfter(TimeSpan.FromSeconds(30), Now.AddSeconds(-20));

        IOperationResult result = Result.FromError(error);

        // Act
        var retryAfter = result.GetRetryAfter(Clock);

        // Assert
        retryAfter.ShouldNotBeNull();
        retryAfter.Delay.ShouldBe(TimeSpan.FromSeconds(30));
        retryAfter.RetryAt.ShouldBe(Now.AddSeconds(10));
        retryAfter.GetRemainingSeconds(Clock).ShouldBe(10L);
    }

    [Fact]
    public void GetRetryAfter_FutureDateWithoutOffsetAndNoTimeProvider_UsesSystemClock()
    {
        // Arrange
        var inOneHour = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, inOneHour);

        IOperationResult result = Result.FromError(error);

        // Act
        var retryAfter = result.GetRetryAfter();

        // Assert
        retryAfter.ShouldNotBeNull();
    }

    [Fact]
    public void GetRetryAfter_NullResult_ThrowsArgumentNullException()
    {
        // Arrange
        IOperationResult result = null!;

        // Act
        var exception = Record.Exception(() => result.GetRetryAfter(Clock));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void IsRetryableError_SuccessfulResult_ReturnsFalse()
    {
        // Arrange
        IOperationResult result = Result.Success();

        // Act
        var isRetryable = result.IsRetryableError(Clock);

        // Assert
        isRetryable.ShouldBeFalse();
    }

    [Fact]
    public void IsRetryableError_SuccessfulResultWithValue_ReturnsFalse()
    {
        // Arrange
        IOperationResult result = Result.FromValue(42);

        // Act
        var isRetryable = result.IsRetryableError(Clock);

        // Assert
        isRetryable.ShouldBeFalse();
    }

    [Fact]
    public void IsRetryableError_FailedResultWithRetryableError_ReturnsTrue()
    {
        // Arrange
        IOperationResult result = Result<string>.FromError(CreateError());

        // Act
        var isRetryable = result.IsRetryableError(Clock);

        // Assert
        isRetryable.ShouldBeTrue();
    }

    [Fact]
    public void IsRetryableError_FailedResultWithFlagFalse_ReturnsFalse()
    {
        // Arrange
        var error = CreateError()
            .WithRetryable(false);

        IOperationResult result = Result.FromError(error);

        // Act
        var isRetryable = result.IsRetryableError(Clock);

        // Assert
        isRetryable.ShouldBeFalse();
    }

    [Theory]
    [InlineData(1999, true)]
    [InlineData(2026, false)]
    public void IsRetryableError_DateWithoutOffset_PassesTimeProvider(int year, bool expected)
    {
        // Arrange (a Validation error is retryable here only by its hint)
        var error = Error.Validation(TypeUri, Title)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2000-01-01T00:00:00");

        IOperationResult result = Result.FromError(error);
        var clock = new FixedTimeProvider(new DateTimeOffset(year, 6, 1, 0, 0, 0, TimeSpan.Zero));

        // Act
        var isRetryable = result.IsRetryableError(clock);

        // Assert
        isRetryable.ShouldBe(expected);
    }

    [Fact]
    public void IsRetryableError_NullResult_ThrowsArgumentNullException()
    {
        // Arrange
        IOperationResult result = null!;

        // Act
        var exception = Record.Exception(() => result.IsRetryableError(Clock));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    #region helper methods

    private static Error CreateError()
        => Error.Unavailable(TypeUri, Title);

    #endregion
}
