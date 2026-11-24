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
using System.Globalization;

namespace Maple.Results.Extensions.Resilience.Abstractions.Helpers;

/// <summary>
///     Reads the stored values of the retry hint. No value makes it throw: the values can come from another service.
/// </summary>
internal static class RetryAfterReader
{
    #region consts

    /// <summary>The number of fraction digits that a <see cref="DateTimeOffset" /> holds (ticks of 100 ns).</summary>
    private const int FractionDigits = 7;

    /// <summary>The preferred format of an HTTP-date (RFC 9110, section 5.6.7), e.g., <c>Wed, 21 Oct 2026 07:28:00 GMT</c>.</summary>
    private const string HttpDateFormat = "r";

    /// <summary>The largest number of whole seconds that a <see cref="TimeSpan" /> holds.</summary>
    private const long MaxSeconds = long.MaxValue / TimeSpan.TicksPerSecond;

    /// <summary>The shape of an offset after the date and time: <c>d</c> is an ASCII digit, any other character stands for itself.</summary>
    private const string OffsetShape = "dd:dd";

    /// <summary>The format of a normalized RFC 3339 text without an offset.</summary>
    private const string Rfc3339Format = "yyyy-MM-dd'T'HH:mm:ss.fffffff";

    /// <summary>The shape of the date and time of an RFC 3339 text: <c>d</c> is an ASCII digit, any other character stands for itself.</summary>
    private const string Rfc3339Shape = "dddd-dd-ddTdd:dd:dd";

    /// <summary>The format of a normalized RFC 3339 text with a numeric offset.</summary>
    private const string Rfc3339WithOffsetFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz";

    #endregion

    /// <summary>
    ///     Reads a stored <see cref="ResilienceMetadataKeys.RetryAfterDate" /> value.
    /// </summary>
    /// <param name="value">The stored value.</param>
    /// <param name="instant">The instant of the value.</param>
    /// <param name="hasOffset">
    ///     Whether the value names its offset (<c>Z</c>, <c>GMT</c>, a numeric offset, a <see cref="DateTimeOffset" />, a <see cref="DateTime" /> of
    ///     kind <see cref="DateTimeKind.Utc" /> or <see cref="DateTimeKind.Local" />); otherwise, the instant assumes UTC.
    /// </param>
    /// <returns><see langword="true" /> if the value has one of the accepted forms.</returns>
    public static bool TryReadDate(object? value, out DateTimeOffset instant, out bool hasOffset)
    {
        switch (value)
        {
            case string text:
                return TryParseRfc3339(text, out instant, out hasOffset) || TryParseHttpDate(text, out instant, out hasOffset);

            case DateTimeOffset dateTimeOffset:
                instant = dateTimeOffset;
                hasOffset = true;

                return true;

            case DateTime { Kind: DateTimeKind.Unspecified } dateTime:
                var assumedUtc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                instant = new DateTimeOffset(assumedUtc);
                hasOffset = false;

                return true;

            case DateTime dateTime:
                // ToUniversalTime clamps instead of throwing at the ends of the range.
                var utc = dateTime.ToUniversalTime();
                instant = new DateTimeOffset(utc);
                hasOffset = true;

                return true;

            default:
                instant = default;
                hasOffset = false;

                return false;
        }
    }

    /// <summary>
    ///     Reads a stored <see cref="ResilienceMetadataKeys.RetryAfterSeconds" /> value: a whole number of seconds that is not negative and fits a
    ///     <see cref="TimeSpan" />, of any integral type, or a <see cref="decimal" />, <see cref="double" /> or <see cref="float" /> without a fraction.
    /// </summary>
    /// <param name="value">The stored value.</param>
    /// <param name="delay">The delay, or <see cref="TimeSpan.Zero" /> if the value is not readable.</param>
    /// <returns><see langword="true" /> if the value is readable.</returns>
    public static bool TryReadSeconds(object? value, out TimeSpan delay)
    {
        switch (value)
        {
            case sbyte number:
                return TryFromSeconds(number, out delay);

            case byte number:
                return TryFromSeconds(number, out delay);

            case short number:
                return TryFromSeconds(number, out delay);

            case ushort number:
                return TryFromSeconds(number, out delay);

            case int number:
                return TryFromSeconds(number, out delay);

            case uint number:
                return TryFromSeconds(number, out delay);

            case long number:
                return TryFromSeconds(number, out delay);

            case ulong number:
                var ulongSeconds = number <= MaxSeconds
                    ? (long)number
                    : -1;

                return TryFromSeconds(ulongSeconds, out delay);

            case decimal number:
                var isWholeDecimal = decimal.IsInteger(number) && number is >= 0 and <= MaxSeconds;
                var decimalSeconds = isWholeDecimal
                    ? (long)number
                    : -1;

                return TryFromSeconds(decimalSeconds, out delay);

            case double number:
                return TryFromWholeDouble(number, out delay);

            case float number:
                return TryFromWholeDouble(number, out delay);

            default:
                delay = TimeSpan.Zero;

                return false;
        }
    }

    #region helper methods

    /// <summary>
    ///     Whether the text has the shape: each <c>d</c> of the shape is an ASCII digit, and every other character is the same.
    /// </summary>
    /// <param name="text">The text; the callers pass exactly the length of the shape.</param>
    /// <param name="shape">The shape.</param>
    private static bool HasShape(ReadOnlySpan<char> text, string shape)
    {
        for (var i = 0; i < shape.Length; i++)
        {
            var matches = shape[i] == 'd'
                ? char.IsAsciiDigit(text[i])
                : text[i] == shape[i];

            if (!matches)
                return false;
        }

        return true;
    }

    private static bool TryFromSeconds(long seconds, out TimeSpan delay)
    {
        if (seconds is < 0 or > MaxSeconds)
        {
            delay = TimeSpan.Zero;

            return false;
        }

        delay = TimeSpan.FromTicks(seconds * TimeSpan.TicksPerSecond);

        return true;
    }

    private static bool TryFromWholeDouble(double number, out TimeSpan delay)
    {
        // IsInteger is false for NaN and the infinities; -0.0 is a whole number that is not negative.
        var isWholeNumber = double.IsInteger(number) && number is >= 0 and <= MaxSeconds;
        var seconds = isWholeNumber
            ? (long)number
            : -1;

        return TryFromSeconds(seconds, out delay);
    }

    /// <summary>
    ///     Parses an HTTP-date in the preferred format of RFC 9110; the obsolete formats are not accepted.
    /// </summary>
    /// <remarks>
    ///     The parse alone ignores the case of the names; formatting the instant back and comparing the texts makes the match exact (the case, the
    ///     padding and the day of the week).
    /// </remarks>
    private static bool TryParseHttpDate(string text, out DateTimeOffset instant, out bool hasOffset)
    {
        hasOffset = true;
        if (!DateTimeOffset.TryParseExact(text, HttpDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out instant))
            return false;

        var formatted = instant.ToString(HttpDateFormat, CultureInfo.InvariantCulture);

        return string.Equals(formatted, text, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Parses an RFC 3339 date and time (section 5.6) with an upper-case <c>T</c> and <c>Z</c>, any fraction of a second, and an optional offset.
    /// </summary>
    /// <remarks>
    ///     The text is checked against the shape first, because the parse methods of .NET alone accept too much (e.g., other separators, a missing time
    ///     or a culture-specific format). The parse then checks the ranges (e.g., the 30th of February, an offset beyond ±14 hours, or an instant outside
    ///     the range of <see cref="DateTimeOffset" />) without throwing.
    /// </remarks>
    private static bool TryParseRfc3339(string text, out DateTimeOffset instant, out bool hasOffset)
    {
        instant = default;
        hasOffset = false;

        var length = Rfc3339Shape.Length;
        if (text.Length < length || !HasShape(text.AsSpan(0, length), Rfc3339Shape))
            return false;

        var dateTime = text[..length];

        // An optional fraction of one digit or more.
        var fraction = string.Empty;
        if (length < text.Length && text[length] == '.')
        {
            var fractionStart = length + 1;
            var fractionEnd = fractionStart;
            while (fractionEnd < text.Length && char.IsAsciiDigit(text[fractionEnd]))
                fractionEnd++;

            if (fractionEnd == fractionStart)
                return false;

            fraction = text[fractionStart..fractionEnd];
            length = fractionEnd;
        }

        // DateTimeOffset holds seven fraction digits: a longer fraction is truncated, a shorter one is padded.
        var fractionText = fraction.Length > FractionDigits
            ? fraction[..FractionDigits]
            : fraction.PadRight(FractionDigits, '0');

        // An optional offset, the rest of the text: Z, or a sign and hh:mm.
        var offset = text.AsSpan(length);
        if (offset.IsEmpty)
        {
            var normalized = $"{dateTime}.{fractionText}";

            return DateTimeOffset.TryParseExact(normalized, Rfc3339Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out instant);
        }

        var isNumericOffset = offset.Length == OffsetShape.Length + 1 && offset[0] is '+' or '-' && HasShape(offset[1..], OffsetShape);
        if (offset is not "Z" && !isNumericOffset)
            return false;

        hasOffset = true;
        var offsetText = isNumericOffset
            ? offset.ToString()
            : "+00:00";

        var normalizedWithOffset = $"{dateTime}.{fractionText}{offsetText}";

        return DateTimeOffset.TryParseExact(normalizedWithOffset, Rfc3339WithOffsetFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out instant);
    }

    #endregion
}
