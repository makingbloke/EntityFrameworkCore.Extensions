// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivityExtensions;

/// <summary>
/// Tests for the GetCaseInsensitivity extension.
/// </summary>
[TestClass]
public class CaseInsensitivityTests
{
    #region public methods

    /// <summary>
    /// Test UseSqliteUnicodeNoCase Guard Clause for a <see cref="DbContextOptionsBuilder"/> object.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase Guard Clause for a DbContextOptionsBuilder object")]
    public void Test_UseSqliteUnicodeNoCase_DbContextOptionsBuilder_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;
        string paramName = "optionsBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => optionsBuilder!.UseSqliteUnicodeNoCase(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation Guard Clause for a <see cref="ModelBuilder"/> object.
    /// </summary>
    [TestMethod("UseSqliteCaseInsensitiveCollation Guard Clause for a ModelBuilder object")]
    public void Test_UseSqliteCaseInsensitiveCollation_ModelBuilder_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;
        string paramName = "modelBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => modelBuilder!.UseSqliteCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation Guard Clause for a <see cref="ModelBuilder"/> object.
    /// </summary>
    [TestMethod("UseSqlServerCaseInsensitiveCollation Guard Clause for a ModelBuilder object")]
    public void Test_UseSqlServerCaseInsensitiveCollation_ModelBuilder_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;
        string paramName = "modelBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => modelBuilder!.UseSqlServerCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase")]
    public void Test_UseSqliteUnicodeNoCase()
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite, useSqliteCaseInsensitivityInterceptor: true);

        // \u00E1 = Latin Small Letter A with Acute.
        string originalValue = new('\u00E1', 10);
        string searchValue = new('A', 10);

        DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        // ASSERT
        List<TestTable1> results = context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToList();

        Assert.IsTrue(results.Count > 0, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase with a non SQLite database type.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase with a non SQLite database type.")]
    public void Test_UseSqliteUnicodeNoCase_NonSqlite()
    {
        // ARRANGE
        string paramName = "connection";

        // ACT / ASSERT
        ArgumentException e = Assert.ThrowsException<ArgumentException>(
            () => {
                using Context context = DatabaseUtils.CreateDatabase(DatabaseType.SqlServer, useSqliteCaseInsensitivityInterceptor: true);
            },
            "Unexpected exception");

        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqlite/SqlServerCaseInsensitiveCollation.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="collation">Default Collation Sequence.</param>
    [TestMethod("UseCaseInsensitiveCollation")]
    [DataRow(DatabaseType.Sqlite, DefaultCollationSequence.SqliteCaseInsensitive, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DefaultCollationSequence.SqlServerCaseInsensitive, DisplayName = DatabaseType.SqlServer)]
    public void Test_UseSqliteCaseInsensitiveCollation(string databaseType, DefaultCollationSequence collation)
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(databaseType, collation: collation);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        // ASSERT
        List<TestTable1> results = context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToList();

        Assert.IsTrue(results.Count > 0, "Invalid record count");
    }

    #endregion public methods
}