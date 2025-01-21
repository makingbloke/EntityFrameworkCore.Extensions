// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;

/// <summary>
/// MSTest Assert Extension Methods.
/// </summary>
public static class AssertExtensions
{
    #region public methods

    /// <summary>
    /// Checks a method throws an exception.
    /// </summary>
    /// <param name="assert">Instance of Assert (<see cref="Assert.That"/> property).</param>
    /// <param name="action">The method to test.</param>
    /// <param name="message">Assertion failed message.</param>
    /// <returns>The exception.</returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This is an extension method of the Assert object.")]
    public static Exception ThrowsException(this Assert assert, Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            return e;
        }

        throw new AssertFailedException(message);
    }

    /// <summary>
    /// Checks a method throws an exception.
    /// </summary>
    /// <param name="assert">Instance of Assert (<see cref="Assert.That"/> property).</param>
    /// <param name="action">The method to test.</param>
    /// <param name="message">Assertion failed message.</param>
    /// <returns>The exception.</returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This is an extension method of the Assert object.")]
    public static async Task<Exception> ThrowsExceptionAsync(this Assert assert, Func<Task> action, string message)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return e;
        }

        throw new AssertFailedException(message);
    }

    #endregion public methods
}
