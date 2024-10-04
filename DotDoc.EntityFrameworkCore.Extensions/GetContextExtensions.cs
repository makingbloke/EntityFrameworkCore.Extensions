// Copyright ©2021-2024 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core Get Context Extensions.
/// </summary>
public static class GetContextExtensions
{
    #region Public GetContext methods

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="IQueryable{T}"/> created by EF Core.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The LINQ query.</param>
    /// <returns><see cref="DbContext"/> or <see langword="null"/> if it is not available (such as object not created by EF core).</returns>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "The context is stored as a private field in IQueryable<> in EF Core 7.")]
    public static DbContext GetContext<TSource>(this IQueryable<TSource> source)
    {
        FieldInfo fieldInfo = source.GetType().GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
        DbContext context = fieldInfo?.GetValue(source) as DbContext;
        return context;
    }

    #endregion Public GetContext methods
}
