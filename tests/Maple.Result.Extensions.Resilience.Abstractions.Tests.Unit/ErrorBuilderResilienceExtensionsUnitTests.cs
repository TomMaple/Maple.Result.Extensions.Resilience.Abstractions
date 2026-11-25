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

namespace Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit;

public class ErrorBuilderResilienceExtensionsUnitTests
{
    private const string Title = "The service is unavailable.";
    private static readonly ErrorUri TypeUri = ErrorUri.Tag("tag:test.com,2024:Test");

    #region WithRetryAfter(TimeSpan)

    [Fact]
    public void WithRetryAfterDelay_WholeSeconds_StoresSecondsAsLong()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120));

        // Assert
        var error = builder.Build();
        error.TryGetMetadata<long>(ResilienceMetadataKeys.RetryAfterSeconds, out var seconds).ShouldBeTrue();
        seconds.ShouldBe(120L);
    }

    [Theory]
    [InlineData(1_200, 2L)]
    [InlineData(1, 1L)]
    [InlineData(1_000, 1L)]
    [InlineData(1_001, 2L)]
    [InlineData(0, 0L)]
    public void WithRetryAfterDelay_Fraction_RoundsUpToWholeSeconds(int milliseconds, long expectedSeconds)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromMilliseconds(milliseconds));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(expectedSeconds);
    }

    [Fact]
    public void WithRetryAfterDelay_OneTickOverWholeSecond_RoundsUp()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(3) + TimeSpan.FromTicks(1));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(4L);
    }

    [Fact]
    public void WithRetryAfterDelay_MaxValue_StoresLargestWholeSecondsOfTimeSpan()
    {
        // Arrange (rounded up, TimeSpan.MaxValue would be one second more than a TimeSpan holds)
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.MaxValue);

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(922_337_203_685L);
    }

    [Fact]
    public void WithRetryAfterDelay_MaxValue_IsReadBack()
    {
        // Arrange
        var error = CreateBuilder()
            .WithRetryAfter(TimeSpan.MaxValue)
            .Build();

        // Act
        var found = error.TryGetRetryAfter(out var retryAfter);

        // Assert
        found.ShouldBeTrue();
        retryAfter.ShouldNotBeNull();
        retryAfter.Delay.ShouldBe(TimeSpan.FromSeconds(922_337_203_685));
    }

    [Fact]
    public void WithRetryAfterDelay_NegativeDelay_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromTicks(-1)));

        // Assert
        exception.ShouldBeOfType<ArgumentOutOfRangeException>().ParamName.ShouldBe("delay");
    }

    [Fact]
    public void WithRetryAfterDelay_NegativeDelay_LeavesBuilderUnchanged()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T07:28:00+00:00")
            .WithMetadata("RetryAfterSeconds", 5);

        // Act
        Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(-1)));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(2);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
        metadata["RetryAfterSeconds"].ShouldBe(5);
    }

    [Fact]
    public void WithRetryAfterDelay_DateSetBefore_RemovesDate()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.ContainsKey(ResilienceMetadataKeys.RetryAfterDate).ShouldBeFalse();
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
    }

    [Fact]
    public void WithRetryAfterDelay_SecondsSetBefore_ReplacesSeconds()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(TimeSpan.FromSeconds(30));

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
    }

    [Theory]
    [InlineData("RetryAfterSeconds")]
    [InlineData("RETRYAFTERSECONDS")]
    [InlineData("RetryAfterDate")]
    [InlineData("RETRYAFTERDATE")]
    public void WithRetryAfterDelay_CaseVariantOfKey_RemovesVariantWithoutThrowing(string caseVariant)
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(caseVariant, 30);

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(120)));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
    }

    [Fact]
    public void WithRetryAfterDelay_ValidDelay_ReturnsSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.WithRetryAfter(TimeSpan.FromSeconds(1));

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void WithRetryAfterDelay_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorBuilder builder = null!;

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(1)));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("builder");
    }

    #endregion

    #region WithRetryAfter(DateTimeOffset)

    [Fact]
    public void WithRetryAfterDate_UtcDate_StoresTextWithZeroOffset()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var error = builder.Build();
        error.TryGetMetadata<string>(ResilienceMetadataKeys.RetryAfterDate, out var date).ShouldBeTrue();
        date.ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_DateWithOffset_StoresUtcText()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 9, 28, 0, TimeSpan.FromHours(2)));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_FractionOfSecond_RoundsUpToNextSecond()
    {
        // Arrange
        var builder = CreateBuilder();
        var retryAt = new DateTimeOffset(2026, 10, 21, 7, 28, 0, 400, TimeSpan.Zero);

        // Act
        builder.WithRetryAfter(retryAt);

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:01+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_OneTickOverWholeSecond_RoundsUpToNextSecond()
    {
        // Arrange
        var builder = CreateBuilder();
        var retryAt = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero).AddTicks(1);

        // Act
        builder.WithRetryAfter(retryAt);

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2027-01-01T00:00:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_PastDate_StoresDate()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2000-01-01T00:00:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_MaxValue_DropsFractionWithoutThrowing()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(DateTimeOffset.MaxValue));

        // Assert
        exception.ShouldBeNull();
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("9999-12-31T23:59:59+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_MinValue_StoresDate()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(DateTimeOffset.MinValue);

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("0001-01-01T00:00:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_SecondsSetBefore_RemovesSeconds()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(TimeSpan.FromSeconds(120));

        // Act
        builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.ContainsKey(ResilienceMetadataKeys.RetryAfterSeconds).ShouldBeFalse();
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Theory]
    [InlineData("RetryAfterSeconds")]
    [InlineData("RETRYAFTERSECONDS")]
    [InlineData("RetryAfterDate")]
    [InlineData("RETRYAFTERDATE")]
    public void WithRetryAfterDate_CaseVariantOfKey_RemovesVariantWithoutThrowing(string caseVariant)
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(caseVariant, "Wed, 21 Oct 2026 07:28:00 GMT");

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_AnyDate_ReturnsSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void WithRetryAfterDate_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorBuilder builder = null!;

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("builder");
    }

    #endregion

    #region WithRetryAfter(TimeSpan, DateTimeOffset)

    [Fact]
    public void WithRetryAfterReceived_WholeSeconds_StoresSecondsAndEndOfWait()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(2);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:30:00+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_FractionOfDelay_AddsRoundedUpSeconds()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromMilliseconds(1_200), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(2L);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:02+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_FractionOfReceivedAt_RoundsDateUp()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 7, 28, 0, 400, TimeSpan.Zero));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:30:01+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_ReceivedAtWithOffset_StoresUtcDate()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 9, 28, 0, TimeSpan.FromHours(2)));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:30:00+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_ZeroDelay_StoresZeroAndReceivedAt()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.Zero, new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(0L);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_PastReceivedAt_StoresPastDate()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(30), new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2000-01-01T00:00:30+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_SumBeyondMaxValue_StoresSecondsOnly()
    {
        // Arrange
        var builder = CreateBuilder();
        var receivedAt = DateTimeOffset.MaxValue.AddSeconds(-10);

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromMinutes(1), receivedAt));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(60L);
    }

    [Fact]
    public void WithRetryAfterReceived_MaxDelay_StoresSecondsOnly()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.MaxValue, new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(922_337_203_685L);
    }

    [Fact]
    public void WithRetryAfterReceived_PositiveOffsetNearMaxValue_StoresUtcDate()
    {
        // Arrange (the instant is in range, the clock time of the offset would not be)
        var builder = CreateBuilder();
        var receivedAt = new DateTimeOffset(9999, 12, 31, 23, 0, 0, TimeSpan.FromHours(14));

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromHours(10), receivedAt));

        // Assert
        exception.ShouldBeNull();
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("9999-12-31T19:00:00+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_SumEqualToLastWholeSecond_StoresDate()
    {
        // Arrange
        var builder = CreateBuilder();
        var receivedAt = new DateTimeOffset(9999, 12, 31, 23, 59, 0, TimeSpan.Zero);

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(59), receivedAt);

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("9999-12-31T23:59:59+00:00");
    }

    [Fact]
    public void WithRetryAfterReceived_NegativeDelay_ThrowsAndLeavesBuilderUnchanged()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, 5L);

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(-1), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeOfType<ArgumentOutOfRangeException>().ParamName.ShouldBe("delay");
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(5L);
    }

    [Theory]
    [InlineData("RetryAfterSeconds")]
    [InlineData("RETRYAFTERSECONDS")]
    [InlineData("retryAfterSeconds")]
    [InlineData("RetryAfterDate")]
    [InlineData("RETRYAFTERDATE")]
    [InlineData("retryAfterDate")]
    public void WithRetryAfterReceived_ExistingKey_ReplacesItWithoutThrowing(string existingKey)
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(existingKey, 30)
            .WithMetadata("traceId", "abc");

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(3);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:30:00+00:00");
        metadata["traceId"].ShouldBe("abc");
    }

    [Fact]
    public void WithRetryAfterReceived_AnyDelay_ReturnsSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.WithRetryAfter(TimeSpan.FromSeconds(1), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void WithRetryAfterReceived_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorBuilder builder = null!;

        // Act
        var exception = Record.Exception(() => builder.WithRetryAfter(TimeSpan.FromSeconds(1), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero)));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("builder");
    }

    [Fact]
    public void WithRetryAfterDelay_AfterReceived_LeavesSecondsOnly()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(30));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(30L);
    }

    [Fact]
    public void WithRetryAfterDate_AfterReceived_LeavesDateOnly()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(TimeSpan.FromSeconds(120), new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Act
        builder.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero));

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T08:00:00+00:00");
    }

    #endregion

    #region all overloads

    [Fact]
    public void WithRetryAfter_OtherMembers_LeavesThemUntouched()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithDetail("Try again later.")
            .WithDetailTemplated("errors.unavailable", ("service", "orders"))
            .WithMetadata("traceId", "abc")
            .WithMetadata("limit", 100);

        var expected = builder.Build();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120))
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        var error = builder.Build();
        error.Category.ShouldBe(expected.Category);
        error.TypeUri.ShouldBe(expected.TypeUri);
        error.Title.ShouldBe(expected.Title);
        error.Detail.ShouldBe(expected.Detail);
        error.DetailTemplated.ShouldBe(expected.DetailTemplated);
        error.Metadata.Count.ShouldBe(3);
        error.Metadata["traceId"].ShouldBe("abc");
        error.Metadata["limit"].ShouldBe(100);
    }

    [Fact]
    public void WithRetryAfter_ErrorBuiltBefore_DoesNotChangeIt()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata("RetryAfterSeconds", 30);

        var builtBefore = builder.Build();

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(120))
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        builtBefore.Metadata.Count.ShouldBe(1);
        builtBefore.Metadata["RetryAfterSeconds"].ShouldBe(30);
    }

    #endregion

    #region WithRetryable

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithRetryable_AnyFlag_StoresBool(bool retryable)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.WithRetryable(retryable);

        // Assert
        var error = builder.Build();
        error.TryGetMetadata<bool>(ResilienceMetadataKeys.Retryable, out var stored).ShouldBeTrue();
        stored.ShouldBe(retryable);
    }

    [Fact]
    public void WithRetryable_SecondCall_ReplacesValue()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryable(true);

        // Act
        builder.WithRetryable(false);

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.Retryable].ShouldBe(false);
    }

    [Theory]
    [InlineData("Retryable")]
    [InlineData("RETRYABLE")]
    [InlineData("retryable")]
    public void WithRetryable_KeyInOtherCase_ReplacesItWithoutThrowing(string existingKey)
    {
        // Arrange
        var builder = CreateBuilder()
            .WithMetadata(existingKey, "yes");

        // Act
        var exception = Record.Exception(() => builder.WithRetryable(true));

        // Assert
        exception.ShouldBeNull();
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(1);
        metadata[ResilienceMetadataKeys.Retryable].ShouldBe(true);
    }

    [Fact]
    public void WithRetryable_OtherMetadata_LeavesItUntouched()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryAfter(TimeSpan.FromSeconds(120))
            .WithMetadata("traceId", "abc");

        // Act
        builder.WithRetryable(false);

        // Assert
        var metadata = builder.Build().Metadata;
        metadata.Count.ShouldBe(3);
        metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
        metadata["traceId"].ShouldBe("abc");
        metadata[ResilienceMetadataKeys.Retryable].ShouldBe(false);
    }

    [Fact]
    public void WithRetryAfter_AfterWithRetryable_KeepsFlag()
    {
        // Arrange
        var builder = CreateBuilder()
            .WithRetryable(true);

        // Act
        builder.WithRetryAfter(TimeSpan.FromSeconds(30));

        // Assert
        builder.Build().Metadata[ResilienceMetadataKeys.Retryable].ShouldBe(true);
    }

    [Fact]
    public void WithRetryable_AnyFlag_ReturnsSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.WithRetryable(true);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void WithRetryable_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorBuilder builder = null!;

        // Act
        var exception = Record.Exception(() => builder.WithRetryable(true));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("builder");
    }

    #endregion

    #region helper methods

    private static ErrorBuilder CreateBuilder()
        => ErrorBuilder.Unavailable(TypeUri, Title);

    #endregion
}
