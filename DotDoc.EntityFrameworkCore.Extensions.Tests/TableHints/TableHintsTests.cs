// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.TableHints;

/// <summary>
/// Tests for the WithTableHints extension.
/// </summary>
[TestClass]
public class TableHintsTests
{
    #region public methods

    /// <summary>
    /// Test UseTableHintExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseTableHintExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void TableHintsTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseTableHintExtensions(), "Unexpected exception");
    }

    /// <summary>
    /// Test UseTableHintExtensions with an unsupported database type (Not SQL Server).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseTableHintExtensions with an unsupported database type")]
    public async Task TableHintsTests_002_Async()
    {
        // ARRANGE / ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.CreateDatabaseAsync(
                    DatabaseTypes.Sqlite,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
                    .ConfigureAwait(false);
            },
            "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test WithTableHints with an empty Table Hint IEnumerable.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with an empty Table Hint IEnumerable")]
    public async Task TableHintsTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with a Null Table Hint.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with a Null Table Hint")]
    public async Task TableHintsTests_004_Async()
    {
        // ARRANGE
        SqlServerTableHint tableHint = null!;

        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHint), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with all valid Table Hints.
    /// </summary>
    /// <param name="tableHint">The Table Hint to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with all valid Table Hints")]
    [DynamicData(nameof(TableHintsSource))]
    public async Task TableHintsTests_005_Async(SqlServerTableHint tableHint)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        string tableHintClause = $"WITH ({tableHint})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHint);
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from generated SQL.");
    }

    /// <summary>
    /// Test WithTableHints with multiple Table Hints.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with multiple Table Hints")]
    public async Task TableHintsTests_006_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        SqlServerTableHint tableHint1 = SqlServerTableHint.NoExpand;
        SqlServerTableHint tableHint2 = SqlServerTableHint.ForceScan;
        SqlServerTableHint tableHint3 = SqlServerTableHint.HoldLock;
        SqlServerTableHint tableHint4 = SqlServerTableHint.NoLock;

        string tableHintClause = $"WITH ({tableHint1}, {tableHint2}, {tableHint3}, {tableHint4})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHint1, tableHint2, tableHint3, tableHint4);
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from generated SQL.");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Return all valid Table Hints.
    /// </summary>
    /// <returns><see cref="IEnumerable{SqlServerTableHint}"/>.</returns>
    private static IEnumerable<SqlServerTableHint> TableHintsSource()
    {
        yield return SqlServerTableHint.NoExpand;
        yield return SqlServerTableHint.ForceScan;
        yield return SqlServerTableHint.HoldLock;
        yield return SqlServerTableHint.NoLock;
        yield return SqlServerTableHint.NoWait;
        yield return SqlServerTableHint.PagLock;
        yield return SqlServerTableHint.ReadCommitted;
        yield return SqlServerTableHint.ReadCommittedLock;
        yield return SqlServerTableHint.ReadPast;
        yield return SqlServerTableHint.ReadUncommitted;
        yield return SqlServerTableHint.RepeatableRead;
        yield return SqlServerTableHint.RowLock;
        yield return SqlServerTableHint.Serializable;
        yield return SqlServerTableHint.Snapshot;
        yield return SqlServerTableHint.TabLock;
        yield return SqlServerTableHint.TabLockX;
        yield return SqlServerTableHint.Updlock;
        yield return SqlServerTableHint.XLock;
        yield return SqlServerTableHint.Index("DummyIndexValue");
        yield return SqlServerTableHint.ForceSeek();
        yield return SqlServerTableHint.ForceSeek("DummyIndexValue", "DummyIndexColumnNames1", "DummyIndexColumnNames2");
        yield return SqlServerTableHint.SpacialWindowMaxCells(100);
    }

    #endregion private methods
}