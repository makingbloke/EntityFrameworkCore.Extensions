// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

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
    /// Test UseTableHintExtensions with an unsupported database type (only SQL Server is currently supported).
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
            "Unexpected exception")
            .ConfigureAwait(false);
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
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
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

        List<SqlServerTableHint> tableHints =
            [
                SqlServerTableHint.NoExpand,
                SqlServerTableHint.ForceScan,
                SqlServerTableHint.HoldLock,
                SqlServerTableHint.NoLock
            ];

        string tableHintClause = $"WITH ({string.Join(", ", tableHints)})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHints);
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter.
    /// </summary>
    /// <param name="indexValues">The index values (names).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter")]
    [DataRow(null!, DisplayName = "Null")]
    [DataRow([], DisplayName = "Empty")]
    public void TableHintsTests_007(IEnumerable<string> indexValues)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlServerTableHint.Index(indexValues), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null IndexValues parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null IndexValues parameter value")]
    public void TableHintsTests_008()
    {
        // ARRANGE
        string indexValue = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHint.Index(indexValue), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value.
    /// </summary>
    /// <param name="indexValue">The index value (name).</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value")]
    [DataRow(null!, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public void TableHintsTests_009(string indexValue, Type exceptionType)
    {
        // ARRANGE
        IEnumerable<string> indexColumnNames = ["DummyIndexColumnName"];

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => SqlServerTableHint.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter.
    /// </summary>
    /// <param name="indexColumnNames">The index values (names).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter")]
    [DataRow(null!, DisplayName = "Null")]
    [DataRow([], DisplayName = "Empty")]
    public void TableHintsTests_010(IEnumerable<string> indexColumnNames)
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlServerTableHint.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value")]
    public void TableHintsTests_011()
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";
        string indexColumnName = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHint.ForceSeek(indexValue, indexColumnName), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.SpacialWindowMaxCells with an Int Value outside the allowed range.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.SpacialWindowMaxCells with an Int Value outside the allowed range")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(8193, DisplayName = "8193")]
    public void TableHintsTests_012(int value)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SqlServerTableHint.SpacialWindowMaxCells(value), "Unexpected exception");
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
        yield return SqlServerTableHint.ForceSeek("DummyIndexValue", "DummyIndexColumnName");
        yield return SqlServerTableHint.SpacialWindowMaxCells(100);
    }

    #endregion private methods
}