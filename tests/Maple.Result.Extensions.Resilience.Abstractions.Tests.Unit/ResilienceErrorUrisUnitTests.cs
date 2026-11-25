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
using System.Linq;
using System.Reflection;

namespace Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit;

/// <summary>
/// The types are part of the wire format: other packages and non-.NET clients recognise an error by them, so a change is a breaking change.
/// </summary>
public class ResilienceErrorUrisUnitTests
{
    #region type URIs

    public static TheoryData<string, ErrorUri, string> TypeUris
        => new()
        {
            { nameof(ResilienceErrorUris.Timeout), ResilienceErrorUris.Timeout, "tag:mapledev.engineer,2026-03-03:result.resilience.timeout" },
            { nameof(ResilienceErrorUris.CircuitOpen), ResilienceErrorUris.CircuitOpen, "tag:mapledev.engineer,2026-03-03:result.resilience.circuitOpen" },
            { nameof(ResilienceErrorUris.RateLimited), ResilienceErrorUris.RateLimited, "tag:mapledev.engineer,2026-03-03:result.resilience.rateLimited" },
        };

    [Theory]
    [MemberData(nameof(TypeUris))]
    public void TypeUri_Member_HasExactText(string property, ErrorUri typeUri, string expected)
    {
        // Act & Assert
        typeUri.Value.ShouldBe(expected, property);
    }

    // A tag URI (RFC 4151): ErrorUri.Tag validates the syntax and throws for any other text.
    [Theory]
    [MemberData(nameof(TypeUris))]
    public void TypeUri_Member_IsTagUri(string property, ErrorUri typeUri, string expected)
    {
        // Act
        var exception = Record.Exception(() => ErrorUri.Tag(expected));

        // Assert
        exception.ShouldBeNull(property);
        ErrorUri.Tag(expected).ShouldBe(typeUri, property);
    }

    // The value parses as the same tag URI, so a type read back from problem details equals the property.
    [Theory]
    [MemberData(nameof(TypeUris))]
    public void TypeUri_Member_EqualsTheParsedValue(string property, ErrorUri typeUri, string expected)
    {
        // Act
        var result = ErrorUri.Parse(expected);

        // Assert
        result.ShouldBe(typeUri, property);
    }

    [Fact]
    public void TypeUris_EveryProperty_HasRow()
    {
        // Arrange
        var rows = TypeUris.Select(row => (string)row.Data.Item1).Order(StringComparer.Ordinal);

        // Act
        var properties = GetPublicStaticNames(typeof(ResilienceErrorUris).GetProperties(BindingFlags.Public | BindingFlags.Static));

        // Assert
        properties.ShouldBe(rows);
    }

    #endregion

    #region pairs with the titles

    public static TheoryData<ErrorCategory, ErrorUri, string> Pairs
        => new()
        {
            { ErrorCategory.Timeout, ResilienceErrorUris.Timeout, ResilienceErrorTitles.Timeout },
            { ErrorCategory.Unavailable, ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen },
            { ErrorCategory.LimitExceeded, ResilienceErrorUris.RateLimited, ResilienceErrorTitles.RateLimited },
        };

    [Theory]
    [MemberData(nameof(Pairs))]
    public void Pair_ErrorBuiltWithItsCategory_IsValid(ErrorCategory category, ErrorUri typeUri, string title)
    {
        // Act
        var error = new ErrorBuilder(category, typeUri, title).Build();

        // Assert
        error.Category.ShouldBe(category);
        error.TypeUri.ShouldBe(typeUri);
        error.Title.ShouldBe(title);
    }

    [Fact]
    public void Pair_CircuitOpenWithFactoryMethod_IsValid()
    {
        // Act
        var error = Error.Unavailable(ResilienceErrorUris.CircuitOpen, ResilienceErrorTitles.CircuitOpen);

        // Assert
        error.Category.ShouldBe(ErrorCategory.Unavailable);
        error.TypeUri.ShouldBe(ResilienceErrorUris.CircuitOpen);
        error.Title.ShouldBe(ResilienceErrorTitles.CircuitOpen);
    }

    // A type goes with the title of the same name: each type URI has a title, and each title a type URI.
    [Fact]
    public void TypeUris_EveryProperty_HasTitleOfSameName()
    {
        // Arrange
        var titles = GetPublicStaticNames(typeof(ResilienceErrorTitles).GetFields(BindingFlags.Public | BindingFlags.Static));

        // Act
        var properties = GetPublicStaticNames(typeof(ResilienceErrorUris).GetProperties(BindingFlags.Public | BindingFlags.Static));

        // Assert
        properties.ShouldBe(titles);
    }

    #endregion

    #region helper methods

    private static string[] GetPublicStaticNames(MemberInfo[] members)
        => [.. members.Select(member => member.Name).Order(StringComparer.Ordinal)];

    #endregion
}
