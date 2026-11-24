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

public class PackageInfoUnitTests
{
    [Fact]
    public void Name_Always_EqualsAssemblyName()
    {
        // Arrange
        var assemblyName = typeof(PackageInfo).Assembly.GetName().Name;

        // Act
        var name = PackageInfo.Name;

        // Assert
        name.ShouldBe(assemblyName);
    }
}
