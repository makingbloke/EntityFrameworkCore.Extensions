// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.SqlParser;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.SqlParser;

/// <summary>
/// Tests for the SQL parser (<see cref="SqlDialect"/> and its database specific implementations).
/// </summary>
/// <remarks>
/// These tests don't need a database, so they check the SQL Server rules even when SQL Server is not available.
/// The tests that run the generated SQL against a database are in ExecutePagedQueryTests.
/// </remarks>
[TestClass]
public class SqlDialectTests
{
    #region public methods

    /// <summary>
    /// Test SqlDialect.For with an unsupported database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    [TestMethod(DisplayName = "SqlDialect.For with an unsupported database type")]
    [DataRow(null, DisplayName = "Null")]
    [DataRow("Oracle", DisplayName = "Oracle")]
    public void SqlDialectTests_001(string? databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => SqlDialect.For(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize with a Null String Sql parameter.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    [TestMethod(DisplayName = "SqlDialect.Tokenize with a Null String Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void SqlDialectTests_002(string databaseType)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => dialect.Tokenize(null!), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize reads each database's strings and quoted identifiers (including escaped quotes) as a single token.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The quoted token.</param>
    /// <param name="expectedType">The expected <see cref="SqlTokenType"/> name.</param>
    [TestMethod(DisplayName = "SqlDialect.Tokenize with strings and quoted identifiers")]
    [DataRow(DatabaseTypes.Sqlite, "'it''s ORDER BY'", "String", DisplayName = $"{DatabaseTypes.Sqlite} 'string'")]
    [DataRow(DatabaseTypes.Sqlite, @"""ORDER """" BY""", "QuotedIdentifier", DisplayName = @$"{DatabaseTypes.Sqlite} ""identifier""")]
    [DataRow(DatabaseTypes.Sqlite, "[ORDER BY]", "QuotedIdentifier", DisplayName = $"{DatabaseTypes.Sqlite} [identifier]")]
    [DataRow(DatabaseTypes.Sqlite, "`ORDER `` BY`", "QuotedIdentifier", DisplayName = $"{DatabaseTypes.Sqlite} `identifier`")]
    [DataRow(DatabaseTypes.SqlServer, "'it''s ORDER BY'", "String", DisplayName = $"{DatabaseTypes.SqlServer} 'string'")]
    [DataRow(DatabaseTypes.SqlServer, @"""ORDER """" BY""", "QuotedIdentifier", DisplayName = @$"{DatabaseTypes.SqlServer} ""identifier""")]
    [DataRow(DatabaseTypes.SqlServer, "[ORDER ]] BY]", "QuotedIdentifier", DisplayName = $"{DatabaseTypes.SqlServer} [identifier]")]
    public void SqlDialectTests_003(string databaseType, string sql, string expectedType)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize(sql);

        // ASSERT
        Assert.HasCount(1, tokens, "Invalid token count");
        Assert.AreEqual(expectedType, tokens[0].Type.ToString(), "Invalid token type");
        Assert.AreEqual(sql, tokens[0].Text, "Invalid token text");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize doesn't treat a backtick as a quote on SQL Server (only SQLite supports `identifier`).
    /// </summary>
    [TestMethod(DisplayName = "SqlDialect.Tokenize with a backtick on SQL Server")]
    public void SqlDialectTests_004()
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.SqlServer);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize("`Name`");

        // ASSERT
        Assert.AreEqual("`|Name|`", TokenTexts(tokens), "Invalid tokens");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize skips line and block comments, and handles nested block comments for each database.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedTokens">The expected token texts, separated by |.</param>
    [TestMethod(DisplayName = "SqlDialect.Tokenize with comments")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT /* a */ 1 -- b\r\n, 2 --", "SELECT|1|,|2", DisplayName = $"{DatabaseTypes.Sqlite} Line and block comments")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT /* a */ 1 -- b\r\n, 2 --", "SELECT|1|,|2", DisplayName = $"{DatabaseTypes.SqlServer} Line and block comments")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a /* b */ c */ FROM T", "SELECT|1|c|*|/|FROM|T", DisplayName = $"{DatabaseTypes.Sqlite} Block comments are not nested")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* a /* b */ c */ FROM T", "SELECT|1|FROM|T", DisplayName = $"{DatabaseTypes.SqlServer} Block comments are nested")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a FROM T", "SELECT|1", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed block comment runs to the end")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a /* b", "SELECT|1", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed block comment containing /*")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /*/ a", "SELECT|1", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed /*/ block comment")]
    public void SqlDialectTests_005(string databaseType, string sql, string expectedTokens)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize(sql);

        // ASSERT
        Assert.AreEqual(expectedTokens, TokenTexts(tokens), "Invalid tokens");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize sets the bracket depth of each token (brackets have the depth of the text outside them).
    /// </summary>
    [TestMethod(DisplayName = "SqlDialect.Tokenize bracket depth")]
    public void SqlDialectTests_006()
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.Sqlite);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize("SELECT (1 + (2)) FROM T");

        // ASSERT
        Assert.AreEqual("SELECT|(|1|+|(|2|)|)|FROM|T", TokenTexts(tokens), "Invalid tokens");
        Assert.AreEqual("0|0|1|1|1|2|1|0|0|0", string.Join("|", tokens.Select(t => t.Depth)), "Invalid depths");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize with SQL that has an unclosed quote, comment or bracket, or an unexpected closing bracket.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    [TestMethod(DisplayName = "SqlDialect.Tokenize with invalid SQL")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 'abc", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed string")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT `abc", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed `identifier`")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT (1", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed bracket")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1)", DisplayName = $"{DatabaseTypes.Sqlite} Unexpected closing bracket")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT [abc", DisplayName = $"{DatabaseTypes.SqlServer} Unclosed [identifier]")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* abc", DisplayName = $"{DatabaseTypes.SqlServer} Unclosed comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* a /* b */", DisplayName = $"{DatabaseTypes.SqlServer} Unclosed nested comment")]
    public void SqlDialectTests_007(string databaseType, string sql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => dialect.Tokenize(sql), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlDialect.Analyse with SQL that is invalid or can't be paged.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    [TestMethod(DisplayName = "SqlDialect.Analyse with SQL that is invalid or can't be paged")]
    [DataRow(DatabaseTypes.Sqlite, " -- comment only", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.Sqlite} No query")]
    [DataRow(DatabaseTypes.Sqlite, ";", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.Sqlite} Semicolon only")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1; SELECT 2", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.Sqlite} Two statements")]
    [DataRow(DatabaseTypes.Sqlite, "UPDATE T SET a = 1", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} Not a SELECT query")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1) DELETE FROM T", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} WITH without a SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T LIMIT 10", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} LIMIT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a LIMIT 10 OFFSET 5", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY and LIMIT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OFFSET 5 ROWS", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} OFFSET")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T FOR XML AUTO", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} FOR XML")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a FOR JSON PATH", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} FOR JSON")]
    public void SqlDialectTests_008(string databaseType, string sql, Type exceptionType)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => dialect.Analyse(sql), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test SqlDialect.Analyse splits the SQL into the correct parts.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedPrefix">The expected prefix (e.g. a WITH clause).</param>
    /// <param name="expectedQuery">The expected main query.</param>
    /// <param name="expectedOrderBy">The expected outer ORDER BY clause, or <see langword="null"/> if there isn't one.</param>
    /// <param name="expectedTail">The expected trailing clause, or <see langword="null"/> if there isn't one.</param>
    [TestMethod(DisplayName = "SqlDialect.Analyse splits the SQL into the correct parts")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a DESC, b", "", "SELECT * FROM T", "ORDER BY a DESC, b", null, DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a = 1", "", "SELECT * FROM T WHERE a = 1", null, null, DisplayName = $"{DatabaseTypes.Sqlite} No ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT * FROM T ORDER BY a) SELECT * FROM x ORDER BY b", "WITH x AS (SELECT * FROM T ORDER BY a) ", "SELECT * FROM x", "ORDER BY b", null, DisplayName = $"{DatabaseTypes.Sqlite} Common table expression")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT ROW_NUMBER() OVER (ORDER BY a) FROM (SELECT * FROM T ORDER BY b LIMIT 1) x", "", "SELECT ROW_NUMBER() OVER (ORDER BY a) FROM (SELECT * FROM T ORDER BY b LIMIT 1) x", null, null, DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY and LIMIT in brackets")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T /* ORDER BY a */ WHERE b = 'ORDER BY c' -- ORDER BY d", "", "SELECT * FROM T /* ORDER BY a */ WHERE b = 'ORDER BY c'", null, null, DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY in comments and strings")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT a FROM T UNION SELECT a FROM U ORDER BY a;", "", "SELECT a FROM T UNION SELECT a FROM U", "ORDER BY a", null, DisplayName = $"{DatabaseTypes.Sqlite} UNION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OPTION (RECOMPILE); -- c", "", "SELECT * FROM T", "ORDER BY a", "OPTION (RECOMPILE)", DisplayName = $"{DatabaseTypes.SqlServer} OPTION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T OPTION (MAXDOP 1)", "", "SELECT * FROM T", null, "OPTION (MAXDOP 1)", DisplayName = $"{DatabaseTypes.SqlServer} OPTION without ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT Offset, [Option], Fetch FROM T", "", "SELECT Offset, [Option], Fetch FROM T", null, null, DisplayName = $"{DatabaseTypes.SqlServer} Columns named like keywords")]
    public void SqlDialectTests_009(string databaseType, string sql, string expectedPrefix, string expectedQuery, string? expectedOrderBy, string? expectedTail)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT
        SqlQueryShape shape = dialect.Analyse(sql);

        // ASSERT
        Assert.AreEqual(expectedPrefix, shape.Prefix, "Invalid prefix");
        Assert.AreEqual(expectedQuery, shape.Query, "Invalid query");
        Assert.AreEqual(expectedOrderBy, shape.OrderByStart is int start ? sql[start..shape.OrderByEnd!.Value] : null, "Invalid ORDER BY");
        Assert.AreEqual(expectedTail, shape.Tail, "Invalid trailing clause");
    }

    /// <summary>
    /// Test the count and paged SQL built by each database.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedCountSql">The expected count SQL (\n is replaced with <see cref="Environment.NewLine"/>).</param>
    /// <param name="expectedPagedSql">The expected paged SQL (\n is replaced with <see cref="Environment.NewLine"/>).</param>
    [TestMethod(DisplayName = "SqlDialect.BuildCountSql and BuildPagedSql")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a = {0} ORDER BY b -- c", "SELECT COUNT(*) FROM (\nSELECT * FROM T WHERE a = {0}\n) AS t", "SELECT * FROM T WHERE a = {0} ORDER BY b\nLIMIT {1} OFFSET {2}", DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T;", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM T\nLIMIT {1} OFFSET {2}", DisplayName = $"{DatabaseTypes.Sqlite} No ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a", "WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a\nLIMIT {1} OFFSET {2}", DisplayName = $"{DatabaseTypes.Sqlite} Common table expression")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a; /* c", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM T ORDER BY a\nLIMIT {1} OFFSET {2}", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed trailing block comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T WHERE a = {0} ORDER BY b DESC", "SELECT COUNT(*) FROM (\nSELECT * FROM T WHERE a = {0}\n) AS t", "SELECT * FROM T WHERE a = {0} ORDER BY b DESC\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"{DatabaseTypes.SqlServer} ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T -- c", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM T\nORDER BY (SELECT NULL)\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"{DatabaseTypes.SqlServer} No ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OPTION (RECOMPILE);", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t\nOPTION (RECOMPILE)", "SELECT * FROM T ORDER BY a\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY\nOPTION (RECOMPILE)", DisplayName = $"{DatabaseTypes.SqlServer} OPTION")]
    [DataRow(DatabaseTypes.SqlServer, "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a", "WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"{DatabaseTypes.SqlServer} Common table expression")]
    public void SqlDialectTests_010(string databaseType, string sql, string expectedCountSql, string expectedPagedSql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);
        SqlQueryShape shape = dialect.Analyse(sql);

        // ACT
        string countSql = dialect.BuildCountSql(shape);
        string pagedSql = dialect.BuildPagedSql(shape, 1, 2);

        // ASSERT
        Assert.AreEqual(expectedCountSql.Replace("\n", Environment.NewLine, StringComparison.Ordinal), countSql, "Invalid count SQL");
        Assert.AreEqual(expectedPagedSql.Replace("\n", Environment.NewLine, StringComparison.Ordinal), pagedSql, "Invalid paged SQL");
    }

    /// <summary>
    /// Test the SQL Server count SQL for TOP queries: the ORDER BY is only kept for TOP ... WITH TIES.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedCountSql">The expected count SQL (\n is replaced with <see cref="Environment.NewLine"/>).</param>
    [TestMethod(DisplayName = "SqlDialect.BuildCountSql with SQL Server TOP queries")]
    [DataRow("SELECT TOP 5 * FROM T ORDER BY a", "SELECT COUNT(*) FROM (\nSELECT TOP 5 * FROM T\n) AS t", DisplayName = "TOP")]
    [DataRow("SELECT TOP (5) PERCENT * FROM T ORDER BY a", "SELECT COUNT(*) FROM (\nSELECT TOP (5) PERCENT * FROM T\n) AS t", DisplayName = "TOP PERCENT")]
    [DataRow("SELECT TOP (5) WITH TIES * FROM T ORDER BY a", "SELECT COUNT(*) FROM (\nSELECT TOP (5) WITH TIES * FROM T ORDER BY a\n) AS t", DisplayName = "TOP WITH TIES")]
    [DataRow("SELECT DISTINCT TOP ({0}) PERCENT WITH TIES a FROM T ORDER BY a", "SELECT COUNT(*) FROM (\nSELECT DISTINCT TOP ({0}) PERCENT WITH TIES a FROM T ORDER BY a\n) AS t", DisplayName = "DISTINCT TOP PERCENT WITH TIES")]
    public void SqlDialectTests_011(string sql, string expectedCountSql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.SqlServer);
        SqlQueryShape shape = dialect.Analyse(sql);

        // ACT
        string countSql = dialect.BuildCountSql(shape);

        // ASSERT
        Assert.AreEqual(expectedCountSql.Replace("\n", Environment.NewLine, StringComparison.Ordinal), countSql, "Invalid count SQL");
    }

    /// <summary>
    /// Test SqlDialect.BuildPagedSql with SQL Server TOP queries, which can't be paged because SQL Server doesn't allow TOP and OFFSET / FETCH together.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    [TestMethod(DisplayName = "SqlDialect.BuildPagedSql with SQL Server TOP queries")]
    [DataRow("SELECT TOP 5 * FROM T ORDER BY a", DisplayName = "TOP")]
    [DataRow("SELECT ALL TOP (5) * FROM T", DisplayName = "ALL TOP")]
    [DataRow("SELECT TOP (5) WITH TIES * FROM T ORDER BY a", DisplayName = "TOP WITH TIES")]
    public void SqlDialectTests_012(string sql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.SqlServer);
        SqlQueryShape shape = dialect.Analyse(sql);

        // ACT / ASSERT
        Assert.ThrowsExactly<NotSupportedException>(() => dialect.BuildPagedSql(shape, 1, 2), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlDialect.BuildCountSql and BuildPagedSql with a Null SqlQueryShape Shape parameter.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    [TestMethod(DisplayName = "SqlDialect.BuildCountSql and BuildPagedSql with a Null SqlQueryShape Shape parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void SqlDialectTests_013(string databaseType)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => dialect.BuildCountSql(null!), "Unexpected exception (BuildCountSql)");
        Assert.ThrowsExactly<ArgumentNullException>(() => dialect.BuildPagedSql(null!, 1, 2), "Unexpected exception (BuildPagedSql)");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Join the text of a list of tokens, separated by |.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <returns>The token texts.</returns>
    private static string TokenTexts(IEnumerable<SqlToken> tokens)
    {
        return string.Join("|", tokens.Select(t => t.Text));
    }

    #endregion private methods
}
