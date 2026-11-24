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
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_SuccessfulResultWithValue_ReturnsNull()
    {
        // Arrange
        IOperationResult result = Result.FromValue(42);

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithoutHint_ReturnsNull()
    {
        // Arrange
        IOperationResult result = Result.FromError(CreateError());

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithUnreadableHint_ReturnsNull()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, "120");

        IOperationResult result = Result.FromError(error);

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithSeconds_ReturnsDelay()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(TimeSpan.FromSeconds(120));

        IOperationResult result = Result<string>.FromError(error);

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBe(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithDate_ReturnsDelayFromClock()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        IOperationResult result = Result.FromError(error);

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBe(TimeSpan.FromMinutes(28));
    }

    [Fact]
    public void GetRetryAfter_FailedResultWithPastDate_ReturnsZero()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 6, 0, 0, TimeSpan.Zero));

        IOperationResult result = Result.FromError(error);

        // Act
        var delay = result.GetRetryAfter(Clock);

        // Assert
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void GetRetryAfter_NoTimeProvider_UsesSystemClock()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(DateTimeOffset.UtcNow.AddHours(1));

        IOperationResult result = Result.FromError(error);

        // Act
        var delay = result.GetRetryAfter();

        // Assert (the writer rounds the date up to the next whole second)
        delay.ShouldNotBeNull();
        delay.Value.ShouldBeInRange(TimeSpan.FromMinutes(59), TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1));
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

    #region helper methods

    private static Error CreateError()
        => Error.Unavailable(TypeUri, Title);

    #endregion
}
