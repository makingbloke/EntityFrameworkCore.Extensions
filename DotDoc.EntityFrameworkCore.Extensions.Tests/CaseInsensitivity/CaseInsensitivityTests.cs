// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

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

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseSqliteUnicodeNoCase(), "Missing exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqliteCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqliteCaseInsensitiveCollation_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = modelBuilder!.UseSqliteCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(nameof(modelBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqlServerCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqlServerCaseInsensitiveCollation_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = modelBuilder!.UseSqlServerCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(nameof(modelBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase")]
    public void Test_UseSqliteUnicodeNoCase()
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(
            DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseSqliteUnicodeNoCase());

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
        InvalidOperationException e = Assert.ThrowsExactly<InvalidOperationException>(
            () =>
            {
                using Context context = DatabaseUtils.CreateDatabase(
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: (optionsBuilder) => optionsBuilder.UseSqliteUnicodeNoCase());
            },
            "Unexpected exception");

        Assert.AreEqual(message, e.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation.
    /// </summary>
    [TestMethod("UseSqliteCaseInsensitiveCollation")]
    public void Test_UseSqliteCaseInsensitiveCollation()
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(
            DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) => modelBuilder.UseSqliteCaseInsensitiveCollation());

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        DatabaseUtils.CreateTestTableEntries(context, originalValue, 1);

        // ASSERT
        List<TestTable1> results = context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToList();

        Assert.IsTrue(results.Count > 0, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation.
    /// </summary>
    [TestMethod("UseSqlServerCaseInsensitiveCollation")]
    public void Test_UseSqlServerCaseInsensitiveCollation()
    {
        // ARRANGE / ACT
        using Context context = DatabaseUtils.CreateDatabase(
            DatabaseTypes.SqlServer,
            customModelCreationActions: (modelBuilder) => modelBuilder.UseSqlServerCaseInsensitiveCollation());

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