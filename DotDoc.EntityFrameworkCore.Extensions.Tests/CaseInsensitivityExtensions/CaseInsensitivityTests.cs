// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivityExtensions;

/// <summary>
/// Tests for the GetCaseInsensitivity extension.
/// </summary>
[TestClass]
public class CaseInsensitivityTests
{
    #region public methods

    /// <summary>
    /// Test UseSqliteUnicodeNoCase Guard Clause.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase Guard Clause")]
    public void Test_UseSqliteUnicodeNoCase_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;
        string paramName = "optionsBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => optionsBuilder!.UseSqliteUnicodeNoCase(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqliteCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqliteCaseInsensitiveCollation_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;
        string paramName = "modelBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => modelBuilder!.UseSqliteCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqlServerCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqlServerCaseInsensitiveCollation_GuardClause()
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

        string originalValue = new('\u00E1', 10);       // \u00E1 = Latin Small Letter A with Acute.
        string searchValue = new('A', 10);

        DatabaseUtils.CreateTestTableEntries(context, originalValue, 1);

        // ASSERT
        List<TestTable1> results = context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToList();

        Assert.IsTrue(results.Count > 0, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase with an unsupported database type.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase with an unsupported database type")]
    public void Test_UseSqliteUnicodeNoCase_NonSqlite()
    {
        // ARRANGE
        string message = "Unsupported database type";

        // ACT / ASSERT
        InvalidOperationException e = Assert.ThrowsException<InvalidOperationException>(() => DatabaseUtils.CreateDatabase(DatabaseType.SqlServer, useSqliteCaseInsensitivityInterceptor: true), "Unexpected exception");
        Assert.AreEqual(message, e.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test UseSqlite/SqlServerCaseInsensitiveCollation.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="collation">Default Collation Sequence.</param>
    [TestMethod("UseCaseInsensitiveCollation")]
    [DataRow(DatabaseType.Sqlite, DefaultCollationSequence.SqliteCaseInsensitive, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DefaultCollationSequence.SqlServerCaseInsensitive, DisplayName = DatabaseType.SqlServer)]
    public void Test_UseCaseInsensitiveCollation(string databaseType, DefaultCollationSequence collation)
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(databaseType, collation: collation);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        DatabaseUtils.CreateTestTableEntries(context, originalValue, 1);

        // ASSERT
        List<TestTable1> results = context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToList();

        Assert.IsTrue(results.Count > 0, "Invalid record count");
    }

    #endregion public methods
}