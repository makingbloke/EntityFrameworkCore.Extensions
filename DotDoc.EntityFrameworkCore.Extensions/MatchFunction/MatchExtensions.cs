// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Extensions.
/// </summary>
public static class MatchExtensions
{
    #region public methods

    /// <summary>
    /// Use the Match Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseMatchExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder
                    .ReplaceService<IMethodCallTranslatorProvider, MatchTranslatorProvider>();
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return optionsBuilder;
    }

    #endregion public methods
}
