// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;

/// <summary>
/// Method type to be tested.
/// </summary>
[Flags]
public enum TestMethodType
{
    /// <summary>
    /// Use method that take a <see langword="params"/> <see cref="T:object[]"/> argument.
    /// </summary>
    Params = 1,

    /// <summary>
    /// Use method that take an <see cref="IEnumerable{Object}"/> argument.
    /// </summary>
    IEnumerable = 2,

    /// <summary>
    /// Use syncronous method.
    /// </summary>
    Sync = 4,

    /// <summary>
    /// Use asynchronous method.
    /// </summary>
    Async = 8
}
