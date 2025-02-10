// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Case Insensitivity Extensions.
/// </summary>
public static class CaseInsensitivityExtensions
{
    #region public methods

    /// <summary>
    /// Use the Sqlite Unicode NoCase replacement.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseSqliteUnicodeNoCase(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        return optionsBuilder.AddInterceptors(SqliteCaseInsensitivityDbConnectionInterceptor.Instance);
    }

    /// <summary>
    /// Use SQLite Case Insensitive Collation.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static ModelBuilder UseSqliteCaseInsensitiveCollation(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // SQLite doesn't have the concept of a global collation setting so set it on every text field.
        // https://github.com/dotnet/efcore/issues/32051
        foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(string)))
        {
            property.SetCollation("NOCASE");
        }

        return modelBuilder;
    }

    /// <summary>
    /// Use SQL Server Case Insensitive Collation.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static ModelBuilder UseSqlServerCaseInsensitiveCollation(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        return modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
    }

    #endregion public methods
}
