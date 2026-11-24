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
/// The keys are part of the wire format: other packages and non-.NET clients read them, so a change is a breaking change.
/// </summary>
public class ResilienceMetadataKeysUnitTests
{
    [Fact]
    public void RetryAfterDate_Always_IsCamelCaseName()
    {
        // Act
        var key = ResilienceMetadataKeys.RetryAfterDate;

        // Assert
        key.ShouldBe("retryAfterDate");
    }

    [Fact]
    public void RetryAfterSeconds_Always_IsCamelCaseName()
    {
        // Act
        var key = ResilienceMetadataKeys.RetryAfterSeconds;

        // Assert
        key.ShouldBe("retryAfterSeconds");
    }
}
