// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// Case Insensitive Collation Extensions.
/// </summary>
public static class CaseInsensitiveCollationExtensions
{
    #region private constants

    /// <summary>
    /// SQL Server Standard Western European collation (v100): Case-Insensitive, Accent-Sensitive, with full UTF-8 and emoji support for VARCHAR.
    /// </summary>
    private const string SqlServerNoCaseCollation = "Latin1_General_100_CI_AS_SC_UTF8";

    #endregion private constants

    #region public methods

    /// <summary>
    /// Use Case Insensitive Collation.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <param name="databaseType">The database type.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="databaseType"/> is not SQLite or SQL Server.</exception>
    /// <remarks>
    /// <para>
    /// On SQL Server it sets the database collation to Latin1_General_100_CI_AS_SC_UTF8 (case insensitive, accent sensitive), which every column
    /// without its own collation uses. SQLite has no database collation, so it sets the NOCASE collation on every string property that doesn't
    /// already have a collation.
    /// </para>
    /// <para>
    /// Comparing two columns with different collations (for example a property given its own collation with UseCollation, or an entity type added
    /// to the model after this is called) behaves differently on the two databases:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// SQLite uses the collation of the left-hand column, and a column without a collation is BINARY. So when only one column is NOCASE,
    /// <c>a.Name == b.Code</c> and <c>b.Code == a.Name</c> can give different results, and a join's results depend on which side each key is on.
    /// With UseSqliteNoCaseReplacement, string methods follow the collation of the column they're called on, so <c>a.Name.Contains(b.Code)</c>
    /// ignores case when a.Name is NOCASE, whatever the collation of b.Code.
    /// </description></item>
    /// <item><description>
    /// SQL Server throws an exception ("Cannot resolve the collation conflict"), for the comparison and for the string methods.
    /// </description></item>
    /// </list>
    /// <para>
    /// To get the same result on both, give one side an explicit collation with <c>EF.Functions.Collate</c> (NOCASE on SQLite,
    /// Latin1_General_100_CI_AS_SC_UTF8 on SQL Server), which takes precedence on both databases, e.g. <c>EF.Functions.Collate(a.Name, "NOCASE") == b.Code</c>.
    /// Comparing a column with a parameter or a constant always uses the column's collation.
    /// </para>
    /// <para>
    /// On SQLite with UseSqliteNoCaseReplacement, the result of Trim, Replace, Substring, ToLower or ToUpper on a NOCASE column, and of a
    /// concatenation, <c>??</c> or conditional expression with a NOCASE operand, is given the NOCASE collation explicitly (<c>COLLATE NOCASE</c>),
    /// as its result has the column's collation on SQL Server. SQLite also gives that collation to a CASE or function call that contains it,
    /// even in a condition or an argument that isn't part of the result, which SQL Server doesn't. So when only some columns are NOCASE,
    /// <c>(a.Name.Trim() == "abc" ? a.Code : "zzz") == "CODE"</c> ignores case on SQLite even though a.Code is BINARY, while SQL Server uses
    /// a.Code's collation. To get the same result on both, give the outer expression an explicit collation with <c>EF.Functions.Collate</c>, e.g.
    /// <c>EF.Functions.Collate(a.Name.Trim() == "abc" ? a.Code : "zzz", "BINARY") == "CODE"</c> on SQLite (on SQL Server, a case sensitive
    /// collation such as Latin1_General_100_CS_AS_SC_UTF8).
    /// </para>
    /// </remarks>
    public static ModelBuilder UseCaseInsensitiveCollation(this ModelBuilder modelBuilder, string? databaseType)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                // SQLite does not support the concept of a global collation setting so set the NOCASE collation
                // on every text field that does not have a collation already, including those of complex types.
                foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(GetColumnProperties)
                    .Where(p => p.ClrType == typeof(string) && p.GetCollation() is null))
                {
                    property.SetCollation(SqliteUnicodeFunctionNames.NoCaseCollation);
                }

                break;

            case DatabaseTypes.SqlServer:
                modelBuilder.UseCollation(SqlServerNoCaseCollation);
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(databaseType));
        }

        return modelBuilder;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Gets the properties of a type that are stored in columns of their own, including those of its complex types.
    /// </summary>
    /// <param name="type">The entity type or complex type.</param>
    /// <returns>The properties.</returns>
    private static IEnumerable<IMutableProperty> GetColumnProperties(IMutableTypeBase type)
    {
        // A complex type can contain other complex types, so they're walked recursively. A complex type mapped to JSON (ToJson) is
        // skipped: its properties are stored inside the JSON column, so they have no column to set a collation on.
        return type.GetProperties()
            .Concat(type.GetComplexProperties()
                .Where(complexProperty => !complexProperty.ComplexType.IsMappedToJson())
                .SelectMany(complexProperty => GetColumnProperties(complexProperty.ComplexType)));
    }

    #endregion private methods
}