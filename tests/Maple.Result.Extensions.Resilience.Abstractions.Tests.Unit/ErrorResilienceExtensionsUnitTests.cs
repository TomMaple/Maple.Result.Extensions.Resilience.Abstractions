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

public class ErrorResilienceExtensionsUnitTests
{
    private const string Title = "The service is unavailable.";
    private static readonly ErrorUri TypeUri = ErrorUri.Tag("tag:test.com,2024:Test");

    // "Now" of every test that reads a date: the stored dates are 28 minutes later (07:28:00 UTC), unless a test says otherwise.
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 7, 0, 0, TimeSpan.Zero);
    private static readonly TimeProvider Clock = new FixedTimeProvider(Now);
    private static readonly TimeSpan TwentyEightMinutes = TimeSpan.FromMinutes(28);

    #region WithRetryAfter

    [Fact]
    public void WithRetryAfterDelay_AnyError_StoresSameValueAsBuilder()
    {
        // Arrange
        var error = CreateError();
        var expected = error.ToBuilder()
            .WithRetryAfter(TimeSpan.FromMilliseconds(1_200))
            .Build();

        // Act
        var result = error.WithRetryAfter(TimeSpan.FromMilliseconds(1_200));

        // Assert
        result.ShouldBe(expected);
        result.Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(2L);
    }

    [Fact]
    public void WithRetryAfterDate_AnyError_StoresSameValueAsBuilder()
    {
        // Arrange
        var error = CreateError();
        var retryAt = new DateTimeOffset(2026, 10, 21, 9, 28, 0, 400, TimeSpan.FromHours(2));
        var expected = error.ToBuilder()
            .WithRetryAfter(retryAt)
            .Build();

        // Act
        var result = error.WithRetryAfter(retryAt);

        // Assert
        result.ShouldBe(expected);
        result.Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:01+00:00");
    }

    [Fact]
    public void WithRetryAfterDelay_ErrorWithDate_ReplacesDateAndLeavesOriginalUnchanged()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata("RetryAfterDate", "2026-10-21T07:28:00+00:00");

        // Act
        var result = error.WithRetryAfter(TimeSpan.FromSeconds(120));

        // Assert
        result.ShouldNotBeSameAs(error);
        result.Metadata.Count.ShouldBe(1);
        result.Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
        error.Metadata.Count.ShouldBe(1);
        error.Metadata["RetryAfterDate"].ShouldBe("2026-10-21T07:28:00+00:00");
    }

    [Fact]
    public void WithRetryAfterDate_ErrorWithSeconds_ReplacesSecondsAndLeavesOriginalUnchanged()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(TimeSpan.FromSeconds(120));

        // Act
        var result = error.WithRetryAfter(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero));

        // Assert
        result.Metadata.Count.ShouldBe(1);
        result.Metadata[ResilienceMetadataKeys.RetryAfterDate].ShouldBe("2026-10-21T07:28:00+00:00");
        error.Metadata.Count.ShouldBe(1);
        error.Metadata[ResilienceMetadataKeys.RetryAfterSeconds].ShouldBe(120L);
    }

    [Fact]
    public void WithRetryAfterDelay_NegativeDelay_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var error = CreateError();

        // Act
        var exception = Record.Exception(() => error.WithRetryAfter(TimeSpan.FromSeconds(-1)));

        // Assert
        exception.ShouldBeOfType<ArgumentOutOfRangeException>().ParamName.ShouldBe("delay");
    }

    [Fact]
    public void WithRetryAfterDelay_NullError_ThrowsArgumentNullException()
    {
        // Arrange
        Error error = null!;

        // Act
        var exception = Record.Exception(() => error.WithRetryAfter(TimeSpan.FromSeconds(1)));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("error");
    }

    [Fact]
    public void WithRetryAfterDate_NullError_ThrowsArgumentNullException()
    {
        // Arrange
        Error error = null!;

        // Act
        var exception = Record.Exception(() => error.WithRetryAfter(Now));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("error");
    }

    #endregion

    #region TryGetRetryAfter: seconds

    public static TheoryData<object, long> ReadableSeconds => new()
    {
        { (sbyte)120, 120L },
        { (byte)120, 120L },
        { (short)120, 120L },
        { (ushort)120, 120L },
        { 120, 120L },
        { 120u, 120L },
        { 120L, 120L },
        { 120UL, 120L },
        { 120m, 120L },
        { 120.0, 120L },
        { 120f, 120L },
        { 0, 0L },
        { 0L, 0L },
        { 0.0, 0L },
        { -0.0, 0L },
        { 922_337_203_685L, 922_337_203_685L },
        { 922_337_203_685UL, 922_337_203_685L },
        { 922_337_203_685m, 922_337_203_685L },
        { 922_337_203_685.0, 922_337_203_685L },
    };

    public static TheoryData<object?> UnreadableSeconds => new()
    {
        1.5,
        1.5m,
        1.5f,
        -5,
        -5L,
        (sbyte)-5,
        (short)-5,
        -5m,
        -5.0,
        -5f,
        double.NaN,
        double.PositiveInfinity,
        double.NegativeInfinity,
        float.NaN,
        float.PositiveInfinity,
        922_337_203_686L,
        922_337_203_686UL,
        922_337_203_686m,
        922_337_203_686.0,
        long.MaxValue,
        ulong.MaxValue,
        decimal.MaxValue,
        double.MaxValue,
        float.MaxValue,
        "120",
        null!, // TheoryData does not take a null row without the operator
        true,
        'x',
        TimeSpan.FromSeconds(120),
    };

    [Theory]
    [MemberData(nameof(ReadableSeconds))]
    public void TryGetRetryAfter_ReadableSeconds_ReturnsDelay(object value, long expectedSeconds)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.FromTicks(expectedSeconds * TimeSpan.TicksPerSecond));
    }

    [Theory]
    [MemberData(nameof(UnreadableSeconds))]
    public void TryGetRetryAfter_UnreadableSeconds_ReturnsFalseAndZero(object? value)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void TryGetRetryAfter_SecondsStoredByWriter_ReturnsDelay()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(TimeSpan.FromMilliseconds(119_500));

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void TryGetRetryAfter_KeyWithOtherSpelling_ReturnsFalse()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata("RetryAfterSeconds", 120L);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    #endregion

    #region TryGetRetryAfter: date

    public static TheoryData<object, TimeSpan> ReadableDates => new()
    {
        // RFC 3339 with an offset
        { "2026-10-21T07:28:00+00:00", TwentyEightMinutes },
        { "2026-10-21T07:28:00Z", TwentyEightMinutes },
        { "2026-10-21T09:28:00+02:00", TwentyEightMinutes },
        { "2026-10-21T02:28:00-05:00", TwentyEightMinutes },
        { "2026-10-21T07:28:00-00:00", TwentyEightMinutes },
        { "2026-10-21T21:28:00+14:00", TwentyEightMinutes },
        { "2026-10-21T07:28:00.5Z", TwentyEightMinutes + TimeSpan.FromMilliseconds(500) },
        { "2026-10-21T09:28:00.123+02:00", TwentyEightMinutes + TimeSpan.FromMilliseconds(123) },
        { "2026-10-21T07:28:00.1234567Z", TwentyEightMinutes + TimeSpan.FromTicks(1_234_567) },
        { "2026-10-21T07:28:00.123456789Z", TwentyEightMinutes + TimeSpan.FromTicks(1_234_567) },
        // RFC 3339 without an offset, in the future
        { "2026-10-21T07:28:00", TwentyEightMinutes },
        { "2026-10-21T07:28:00.5", TwentyEightMinutes + TimeSpan.FromMilliseconds(500) },
        // HTTP-date
        { "Wed, 21 Oct 2026 07:28:00 GMT", TwentyEightMinutes },
        // Typed values
        { new DateTimeOffset(2026, 10, 21, 9, 28, 0, TimeSpan.FromHours(2)), TwentyEightMinutes },
        { new DateTime(2026, 10, 21, 7, 28, 0, DateTimeKind.Utc), TwentyEightMinutes },
        { new DateTime(2026, 10, 21, 7, 28, 0, DateTimeKind.Utc).ToLocalTime(), TwentyEightMinutes },
        { new DateTime(2026, 10, 21, 7, 28, 0, DateTimeKind.Unspecified), TwentyEightMinutes },
    };

    public static TheoryData<object?> UnreadableDates => new()
    {
        "10/21/2026",
        "tomorrow",
        "",
        "2026-10-21",
        "2026-10-21T07:28Z",
        "2026-10-21 07:28:00Z",
        "2026-10-21t07:28:00z",
        "2026-10-21T07:28:00z",
        " 2026-10-21T07:28:00Z",
        "2026-10-21T07:28:00Z ",
        "2026-10-21T07:28:00Z\n",
        "2026-10-21T07:28:00.Z",
        "2026-10-21T07:28:00+0200",
        "2026-10-21T07:28:00+02",
        "2026-10-21T07:28:00*02:00",
        "2026-10-21T07:28:00+02-00",
        "2026-10-21T07:28:00+15:00",
        "2026-13-21T07:28:00Z",
        "2026-02-30T07:28:00Z",
        "2026-10-21T24:00:00Z",
        "2026-10-21T07:60:00Z",
        "2026-10-21T07:28:60Z",
        "0000-10-21T07:28:00Z",
        "9999-12-31T23:59:59-01:00",
        "２０２６-10-21T07:28:00Z",
        "Thu, 21 Oct 2026 07:28:00 GMT",
        "wed, 21 oct 2026 07:28:00 gmt",
        "Wed, 21 Oct 2026 07:28:00 UTC",
        "Wed, 21 Oct 2026 07:28:00 +0000",
        "Wed, 21 Oct 2026 07:28 GMT",
        "Wednesday, 21-Oct-26 07:28:00 GMT",
        "Wed Oct 21 07:28:00 2026",
        1_761_031_680L,
        null!, // TheoryData does not take a null row without the operator
        true,
        TimeSpan.FromMinutes(28),
    };

    [Theory]
    [MemberData(nameof(ReadableDates))]
    public void TryGetRetryAfter_ReadableDate_ReturnsDelayUntilDate(object value, TimeSpan expectedDelay)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(expectedDelay);
    }

    [Theory]
    [MemberData(nameof(UnreadableDates))]
    public void TryGetRetryAfter_UnreadableDate_ReturnsFalseAndZero(object? value)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    public static TheoryData<object> PastDatesWithOffset => new()
    {
        "2026-10-21T06:00:00Z",
        "2026-10-21T06:00:00+00:00",
        "2026-10-21T08:00:00+02:00",
        "Wed, 21 Oct 2026 06:00:00 GMT",
        "2026-10-21T07:00:00Z",
        "0001-01-01T00:00:00Z",
        new DateTimeOffset(2026, 10, 21, 6, 0, 0, TimeSpan.Zero),
        new DateTime(2026, 10, 21, 6, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 21, 6, 0, 0, DateTimeKind.Utc).ToLocalTime(),
        DateTimeOffset.MinValue,
    };

    public static TheoryData<object> PastDatesWithoutOffset => new()
    {
        "2026-10-21T06:00:00",
        "2026-10-21T07:00:00",
        new DateTime(2026, 10, 21, 6, 0, 0, DateTimeKind.Unspecified),
        DateTime.MinValue,
    };

    [Theory]
    [MemberData(nameof(PastDatesWithOffset))]
    public void TryGetRetryAfter_PastDateWithOffset_ReturnsTrueAndZero(object value)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [MemberData(nameof(PastDatesWithoutOffset))]
    public void TryGetRetryAfter_PastDateWithoutOffset_ReturnsFalse(object value)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, value);

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void TryGetRetryAfter_DateWithOffsetAndItsUtcText_ReturnSameDelay()
    {
        // Arrange
        var withOffset = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T09:28:00+02:00");

        var utc = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T07:28:00+00:00");

        // Act
        withOffset.TryGetRetryAfter(out var delayWithOffset, Clock);
        utc.TryGetRetryAfter(out var delayUtc, Clock);

        // Assert
        delayWithOffset.ShouldBe(delayUtc);
    }

    [Fact]
    public void TryGetRetryAfter_DateStoredByWriter_ReturnsDelay()
    {
        // Arrange
        var error = CreateError()
            .WithRetryAfter(new DateTimeOffset(2026, 10, 21, 9, 28, 0, 400, TimeSpan.FromHours(2)));

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TwentyEightMinutes + TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TryGetRetryAfter_NoTimeProvider_UsesSystemClock()
    {
        // Arrange
        var retryAt = DateTimeOffset.UtcNow.AddHours(1);
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, retryAt);

        // Act
        var found = error.TryGetRetryAfter(out var delay);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBeInRange(TimeSpan.FromMinutes(59), TimeSpan.FromHours(1));
    }

    #endregion

    #region TryGetRetryAfter: both keys

    [Fact]
    public void TryGetRetryAfter_ReadableSecondsAndLaterDate_ReturnsSeconds()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, 30L)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T09:00:00Z");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TryGetRetryAfter_ReadableSecondsAndEarlierDate_ReturnsSeconds()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, 3_600L)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T07:01:00Z");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.FromHours(1));
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData("30")]
    [InlineData(null)]
    public void TryGetRetryAfter_UnreadableSecondsAndReadableDate_ReturnsDate(object? seconds)
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, seconds)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T07:28:00+00:00");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TwentyEightMinutes);
    }

    [Fact]
    public void TryGetRetryAfter_ReadableSecondsAndUnreadableDate_ReturnsSeconds()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, 30)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "tomorrow");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeTrue();
        delay.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TryGetRetryAfter_NeitherReadable_ReturnsFalseAndZero()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata(ResilienceMetadataKeys.RetryAfterSeconds, -1)
            .WithMetadata(ResilienceMetadataKeys.RetryAfterDate, "2026-10-21T06:00:00");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void TryGetRetryAfter_NoHint_ReturnsFalseAndZero()
    {
        // Arrange
        var error = CreateError()
            .WithMetadata("traceId", "abc");

        // Act
        var found = error.TryGetRetryAfter(out var delay, Clock);

        // Assert
        found.ShouldBeFalse();
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void TryGetRetryAfter_NullError_ThrowsArgumentNullException()
    {
        // Arrange
        Error error = null!;

        // Act
        var exception = Record.Exception(() => error.TryGetRetryAfter(out _, Clock));

        // Assert
        exception.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("error");
    }

    #endregion

    #region helper methods

    private static Error CreateError()
        => Error.Unavailable(TypeUri, Title);

    #endregion
}
