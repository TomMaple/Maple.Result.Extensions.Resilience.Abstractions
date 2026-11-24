// SPDX-License-Identifier: MIT
/*
 * This code is a part of a Maple.Result.Extensions.Resilience.Abstractions library project.
 * https://github.com/TomMaple/Maple.Result.Extensions.Resilience.Abstractions/
 * Copyright (c) Tom Maple
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Maple.Results.Extensions.Resilience.Abstractions;

/// <summary>
///     The keys of the <see cref="Error.Metadata" /> entries that describe how an operation that failed can be retried.
/// </summary>
/// <remarks>
///     <para>
///         The keys and the types and formats of their values are a contract with other packages (e.g., the HTTP packages of Maple.Result, which write
///         the <c>Retry-After</c> header of a response) and with non-.NET clients that read a serialized error, so they never change.
///     </para>
///     <para>
///         <see cref="Error.TryGetMetadata{T}" /> does not convert a value: read each key with the type that its documentation names. An error carries at
///         most one of the retry hints; <see cref="ErrorBuilderResilienceExtensions" /> writes one and removes the other.
///     </para>
/// </remarks>
public static class ResilienceMetadataKeys
{
    /// <summary>
    ///     The key of the earliest time to retry the operation: a <see cref="string" /> with a UTC date and time in whole seconds, in the format
    ///     <c>yyyy-MM-dd'T'HH:mm:sszzz</c> (invariant culture), e.g., <c>2026-10-21T07:28:00+00:00</c>.
    /// </summary>
    /// <remarks>
    ///     The offset is always <c>+00:00</c>, never <c>Z</c>: by RFC 9557, <c>+00:00</c> says that UTC is the reference, while <c>Z</c> says that the local
    ///     offset is unknown. The date may be in the past.
    /// </remarks>
    public const string RetryAfterDate = "retryAfterDate";

    /// <summary>
    ///     The key of the delay before the operation is retried: a <see cref="long" /> with the number of whole seconds, zero or more, e.g., <c>120</c>.
    /// </summary>
    public const string RetryAfterSeconds = "retryAfterSeconds";
}
