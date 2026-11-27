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
    #region For tests

    /// <summary>
    /// Test SqlDialect.For with an unsupported database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "UnsupportedDatabaseType Null")]
    [DataRow("Oracle", DisplayName = "UnsupportedDatabaseType Oracle")]
    public void For_UnsupportedDatabaseType_ThrowsUnsupportedDatabaseTypeException(string? databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => SqlDialect.For(databaseType), "Unexpected exception");
    }

    #endregion For tests

    #region Tokenize tests

    /// <summary>
    /// Test SqlDialect.Tokenize with a Null String Sql parameter.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NullSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NullSql {DatabaseTypes.SqlServer}")]
    public void Tokenize_NullSql_ThrowsArgumentNullException(string databaseType)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "'it''s ORDER BY'", "String", DisplayName = $"QuotedText {DatabaseTypes.Sqlite} 'string'")]
    [DataRow(DatabaseTypes.Sqlite, @"""ORDER """" BY""", "QuotedIdentifier", DisplayName = @$"QuotedText {DatabaseTypes.Sqlite} ""identifier""")]
    [DataRow(DatabaseTypes.Sqlite, "[ORDER BY]", "QuotedIdentifier", DisplayName = $"QuotedText {DatabaseTypes.Sqlite} [identifier]")]
    [DataRow(DatabaseTypes.Sqlite, "`ORDER `` BY`", "QuotedIdentifier", DisplayName = $"QuotedText {DatabaseTypes.Sqlite} `identifier`")]
    [DataRow(DatabaseTypes.SqlServer, "'it''s ORDER BY'", "String", DisplayName = $"QuotedText {DatabaseTypes.SqlServer} 'string'")]
    [DataRow(DatabaseTypes.SqlServer, @"""ORDER """" BY""", "QuotedIdentifier", DisplayName = @$"QuotedText {DatabaseTypes.SqlServer} ""identifier""")]
    [DataRow(DatabaseTypes.SqlServer, "[ORDER ]] BY]", "QuotedIdentifier", DisplayName = $"QuotedText {DatabaseTypes.SqlServer} [identifier]")]
    public void Tokenize_QuotedText_ReturnsSingleToken(string databaseType, string sql, string expectedType)
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
    [TestMethod]
    public void Tokenize_SqlServerBacktick_ReturnsSeparateTokens()
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.SqlServer);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize("`Name`");

        // ASSERT
        Assert.AreEqual("`|Name|`", TokenTexts(tokens), "Invalid tokens");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize reads each form of SQLite parameter as a single word, so its name can't be taken for a keyword.
    /// </summary>
    /// <param name="sql">The parameter.</param>
    [TestMethod]
    [DataRow(":limit", DisplayName = "SqliteParameter :name")]
    [DataRow("$limit", DisplayName = "SqliteParameter $name")]
    [DataRow("@limit", DisplayName = "SqliteParameter @name")]
    [DataRow(":1", DisplayName = "SqliteParameter :name starting with a digit")]
    [DataRow("?12", DisplayName = "SqliteParameter ?NNN")]
    [DataRow("?", DisplayName = "SqliteParameter ?")]
    public void Tokenize_SqliteParameter_ReturnsSingleWord(string sql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(DatabaseTypes.Sqlite);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize(sql);

        // ASSERT
        Assert.HasCount(1, tokens, "Invalid token count");
        Assert.AreEqual(SqlTokenType.Word, tokens[0].Type, "Invalid token type");
        Assert.AreEqual(sql, tokens[0].Text, "Invalid token text");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize reads a parameter in a statement as one word on SQLite, and a : or $ without a name, or a :name on SQL Server
    /// (which has no :name parameters), as separate tokens.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedTokens">The expected token texts, separated by |.</param>
    /// <param name="expectedWords">The expected texts of the tokens that are words, separated by |.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "WHERE a < :limit AND b = $offset AND c = ?2", "WHERE|a|<|:limit|AND|b|=|$offset|AND|c|=|?2", "WHERE|a|:limit|AND|b|$offset|AND|c|?2", DisplayName = $"Parameters {DatabaseTypes.Sqlite} In a statement")]
    [DataRow(DatabaseTypes.Sqlite, "a(:p,?1)", "a|(|:p|,|?1|)", "a|:p|?1", DisplayName = $"Parameters {DatabaseTypes.Sqlite} Followed by symbols")]
    [DataRow(DatabaseTypes.Sqlite, ": limit $ offset", ":|limit|$|offset", ":|limit|$|offset", DisplayName = $"Parameters {DatabaseTypes.Sqlite} Prefix without a name")]
    [DataRow(DatabaseTypes.SqlServer, ":limit", ":|limit", "limit", DisplayName = $"Parameters {DatabaseTypes.SqlServer} :name is not a parameter")]
    public void Tokenize_Parameters_ReadsSqliteParametersAsOneWord(string databaseType, string sql, string expectedTokens, string expectedWords)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT
        IReadOnlyList<SqlToken> tokens = dialect.Tokenize(sql);

        // ASSERT
        Assert.AreEqual(expectedTokens, TokenTexts(tokens), "Invalid tokens");
        Assert.AreEqual(expectedWords, TokenTexts(tokens.Where(t => t.Type == SqlTokenType.Word)), "Invalid words");
    }

    /// <summary>
    /// Test SqlDialect.Tokenize skips line and block comments, and handles nested block comments for each database.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedTokens">The expected token texts, separated by |.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "SELECT /* a */ 1 -- b\r\n, 2 --", "SELECT|1|,|2", DisplayName = $"Comments {DatabaseTypes.Sqlite} Line and block comments")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT /* a */ 1 -- b\r\n, 2 --", "SELECT|1|,|2", DisplayName = $"Comments {DatabaseTypes.SqlServer} Line and block comments")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a /* b */ c */ FROM T", "SELECT|1|c|*|/|FROM|T", DisplayName = $"Comments {DatabaseTypes.Sqlite} Block comments are not nested")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* a /* b */ c */ FROM T", "SELECT|1|FROM|T", DisplayName = $"Comments {DatabaseTypes.SqlServer} Block comments are nested")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a FROM T", "SELECT|1", DisplayName = $"Comments {DatabaseTypes.Sqlite} Unclosed block comment runs to the end")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /* a /* b", "SELECT|1", DisplayName = $"Comments {DatabaseTypes.Sqlite} Unclosed block comment containing /*")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1 /*/ a", "SELECT|1", DisplayName = $"Comments {DatabaseTypes.Sqlite} Unclosed /*/ block comment")]
    public void Tokenize_Comments_SkipsComments(string databaseType, string sql, string expectedTokens)
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
    [TestMethod]
    public void Tokenize_Brackets_SetsDepth()
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 'abc", DisplayName = $"InvalidSql {DatabaseTypes.Sqlite} Unclosed string")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT `abc", DisplayName = $"InvalidSql {DatabaseTypes.Sqlite} Unclosed `identifier`")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT (1", DisplayName = $"InvalidSql {DatabaseTypes.Sqlite} Unclosed bracket")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1)", DisplayName = $"InvalidSql {DatabaseTypes.Sqlite} Unexpected closing bracket")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT [abc", DisplayName = $"InvalidSql {DatabaseTypes.SqlServer} Unclosed [identifier]")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* abc", DisplayName = $"InvalidSql {DatabaseTypes.SqlServer} Unclosed comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT 1 /* a /* b */", DisplayName = $"InvalidSql {DatabaseTypes.SqlServer} Unclosed nested comment")]
    public void Tokenize_InvalidSql_ThrowsArgumentException(string databaseType, string sql)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => dialect.Tokenize(sql), "Unexpected exception");
    }

    #endregion Tokenize tests

    #region Analyse tests

    /// <summary>
    /// Test SqlDialect.Analyse with SQL that is invalid or can't be paged.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, " -- comment only", typeof(ArgumentException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} No query")]
    [DataRow(DatabaseTypes.Sqlite, ";", typeof(ArgumentException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} Semicolon only")]
    [DataRow(DatabaseTypes.SqlServer, "; ; -- c", typeof(ArgumentException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} Semicolons only")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT 1; SELECT 2", typeof(ArgumentException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} Two statements")]
    [DataRow(DatabaseTypes.SqlServer, ";SELECT 1; SELECT 2", typeof(ArgumentException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} Two statements after a leading semicolon")]
    [DataRow(DatabaseTypes.SqlServer, ";UPDATE T SET a = 1", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} Not a SELECT query after a leading semicolon")]
    [DataRow(DatabaseTypes.SqlServer, ";WITH x AS (SELECT 1 AS a) INSERT INTO T (a) SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} ;WITH and INSERT ... SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "UPDATE T SET a = 1", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} Not a SELECT query")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1) DELETE FROM T", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH without a SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1 AS a) INSERT INTO T (a) SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH and INSERT ... SELECT")]
    [DataRow(DatabaseTypes.SqlServer, "WITH x AS (SELECT 1 AS a) INSERT INTO T (a) SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} WITH and INSERT ... SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1 AS a) REPLACE INTO T (a) SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH and REPLACE ... SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1 AS a) INSERT OR REPLACE INTO T (a) SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH and INSERT OR REPLACE ... SELECT")]
    [DataRow(DatabaseTypes.Sqlite, "WITH", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH only")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x (SELECT 1) SELECT * FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH without AS")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS SELECT 1", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH without brackets")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS NOT (SELECT 1) SELECT * FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH and NOT without MATERIALIZED")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT 1) y AS (SELECT 2) SELECT * FROM y", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} WITH without a comma")]
    [DataRow(DatabaseTypes.SqlServer, "WITH x AS (SELECT 1 AS a), SELECT a FROM x", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} WITH and a trailing comma")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T LIMIT 10", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} LIMIT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a LIMIT 10 OFFSET 5", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.Sqlite} ORDER BY and LIMIT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OFFSET 5 ROWS", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} OFFSET")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T FOR XML AUTO", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} FOR XML")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a FOR JSON PATH", typeof(NotSupportedException), DisplayName = $"InvalidOrUnpageableSql {DatabaseTypes.SqlServer} FOR JSON")]
    public void Analyse_InvalidOrUnpageableSql_Throws(string databaseType, string sql, Type exceptionType)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a DESC, b", "", "SELECT * FROM T", "ORDER BY a DESC, b", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a = 1", "", "SELECT * FROM T WHERE a = 1", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} No ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT * FROM T ORDER BY a) SELECT * FROM x ORDER BY b", "WITH x AS (SELECT * FROM T ORDER BY a) ", "SELECT * FROM x", "ORDER BY b", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Common table expression")]
    [DataRow(DatabaseTypes.Sqlite, "WITH RECURSIVE x(n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM x WHERE n < 5) SELECT n FROM x ORDER BY n", "WITH RECURSIVE x(n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM x WHERE n < 5) ", "SELECT n FROM x", "ORDER BY n", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Recursive common table expression with a column list")]
    [DataRow(DatabaseTypes.Sqlite, "WITH a AS MATERIALIZED (SELECT 1 AS v), b AS NOT MATERIALIZED (SELECT v FROM a) SELECT v FROM b", "WITH a AS MATERIALIZED (SELECT 1 AS v), b AS NOT MATERIALIZED (SELECT v FROM a) ", "SELECT v FROM b", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} MATERIALIZED and NOT MATERIALIZED common table expressions")]
    [DataRow(DatabaseTypes.Sqlite, "WITH Merge AS (SELECT 1 AS v) SELECT v FROM Merge", "WITH Merge AS (SELECT 1 AS v) ", "SELECT v FROM Merge", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Common table expression named Merge")]
    [DataRow(DatabaseTypes.SqlServer, @"WITH [a] (v) AS (SELECT 1), ""b"" AS (SELECT v FROM [a]) SELECT v FROM ""b"" ORDER BY v", @"WITH [a] (v) AS (SELECT 1), ""b"" AS (SELECT v FROM [a]) ", @"SELECT v FROM ""b""", "ORDER BY v", null, DisplayName = $"PageableSql {DatabaseTypes.SqlServer} Common table expressions with quoted names and a column list")]
    [DataRow(DatabaseTypes.SqlServer, "WITH Replace AS (SELECT 1 AS v) SELECT v FROM Replace", "WITH Replace AS (SELECT 1 AS v) ", "SELECT v FROM Replace", null, null, DisplayName = $"PageableSql {DatabaseTypes.SqlServer} Common table expression named Replace")]
    [DataRow(DatabaseTypes.SqlServer, ";WITH x AS (SELECT 1 AS v) SELECT v FROM x ORDER BY v", ";WITH x AS (SELECT 1 AS v) ", "SELECT v FROM x", "ORDER BY v", null, DisplayName = $"PageableSql {DatabaseTypes.SqlServer} ;WITH")]
    [DataRow(DatabaseTypes.Sqlite, "-- c\n; ;WITH RECURSIVE x AS (SELECT 1 AS v) SELECT v FROM x", "-- c\n; ;WITH RECURSIVE x AS (SELECT 1 AS v) ", "SELECT v FROM x", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} A comment and several semicolons before WITH RECURSIVE")]
    [DataRow(DatabaseTypes.Sqlite, ";SELECT * FROM T ORDER BY a;", ";", "SELECT * FROM T", "ORDER BY a", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Leading and trailing semicolons")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT ROW_NUMBER() OVER (ORDER BY a) FROM (SELECT * FROM T ORDER BY b LIMIT 1) x", "", "SELECT ROW_NUMBER() OVER (ORDER BY a) FROM (SELECT * FROM T ORDER BY b LIMIT 1) x", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} ORDER BY and LIMIT in brackets")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T /* ORDER BY a */ WHERE b = 'ORDER BY c' -- ORDER BY d", "", "SELECT * FROM T /* ORDER BY a */ WHERE b = 'ORDER BY c'", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} ORDER BY in comments and strings")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT a FROM T UNION SELECT a FROM U ORDER BY a;", "", "SELECT a FROM T UNION SELECT a FROM U", "ORDER BY a", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} UNION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OPTION (RECOMPILE); -- c", "", "SELECT * FROM T", "ORDER BY a", "OPTION (RECOMPILE)", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} OPTION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T OPTION (MAXDOP 1)", "", "SELECT * FROM T", null, "OPTION (MAXDOP 1)", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} OPTION without ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT Offset, [Option], Fetch FROM T", "", "SELECT Offset, [Option], Fetch FROM T", null, null, DisplayName = $"PageableSql {DatabaseTypes.SqlServer} Columns named like keywords")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a < :limit", "", "SELECT * FROM T WHERE a < :limit", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} :limit parameter")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a < $limit", "", "SELECT * FROM T WHERE a < $limit", null, null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} $limit parameter")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a < :limit ORDER BY a, :limit", "", "SELECT * FROM T WHERE a < :limit", "ORDER BY a, :limit", null, DisplayName = $"PageableSql {DatabaseTypes.Sqlite} :limit parameter after ORDER BY")]
    public void Analyse_PageableSql_SplitsIntoParts(string databaseType, string sql, string expectedPrefix, string expectedQuery, string? expectedOrderBy, string? expectedTail)
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

    #endregion Analyse tests

    #region BuildCountSql and BuildPagedSql tests

    /// <summary>
    /// Test the count and paged SQL built by each database.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL.</param>
    /// <param name="expectedCountSql">The expected count SQL (\n is replaced with <see cref="Environment.NewLine"/>).</param>
    /// <param name="expectedPagedSql">The expected paged SQL (\n is replaced with <see cref="Environment.NewLine"/>).</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T WHERE a = {0} ORDER BY b -- c", "SELECT COUNT(*) FROM (\nSELECT * FROM T WHERE a = {0}\n) AS t", "SELECT * FROM T WHERE a = {0} ORDER BY b\nLIMIT {1} OFFSET {2}", DisplayName = $"PageableSql {DatabaseTypes.Sqlite} ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T;", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM T\nLIMIT {1} OFFSET {2}", DisplayName = $"PageableSql {DatabaseTypes.Sqlite} No ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a", "WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a\nLIMIT {1} OFFSET {2}", DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Common table expression")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM T ORDER BY a; /* c", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM T ORDER BY a\nLIMIT {1} OFFSET {2}", DisplayName = $"PageableSql {DatabaseTypes.Sqlite} Unclosed trailing block comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T WHERE a = {0} ORDER BY b DESC", "SELECT COUNT(*) FROM (\nSELECT * FROM T WHERE a = {0}\n) AS t", "SELECT * FROM T WHERE a = {0} ORDER BY b DESC\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T -- c", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t", "SELECT * FROM (\nSELECT * FROM T\n) AS t\nORDER BY (SELECT NULL)\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} No ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "WITH x AS (SELECT * FROM T) SELECT a FROM x UNION SELECT a FROM U OPTION (MAXDOP 1)", "WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT a FROM x UNION SELECT a FROM U\n) AS t\nOPTION (MAXDOP 1)", "WITH x AS (SELECT * FROM T) SELECT * FROM (\nSELECT a FROM x UNION SELECT a FROM U\n) AS t\nORDER BY (SELECT NULL)\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY\nOPTION (MAXDOP 1)", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} UNION with a common table expression and OPTION, without ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM T ORDER BY a OPTION (RECOMPILE);", "SELECT COUNT(*) FROM (\nSELECT * FROM T\n) AS t\nOPTION (RECOMPILE)", "SELECT * FROM T ORDER BY a\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY\nOPTION (RECOMPILE)", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} OPTION")]
    [DataRow(DatabaseTypes.SqlServer, "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a", "WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", "WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} Common table expression")]
    [DataRow(DatabaseTypes.SqlServer, ";WITH x AS (SELECT * FROM T) SELECT * FROM x", ";WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", ";WITH x AS (SELECT * FROM T) SELECT * FROM (\nSELECT * FROM x\n) AS t\nORDER BY (SELECT NULL)\nOFFSET {2} ROWS FETCH NEXT {1} ROWS ONLY", DisplayName = $"PageableSql {DatabaseTypes.SqlServer} ;WITH without ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, ";WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a", ";WITH x AS (SELECT * FROM T) SELECT COUNT(*) FROM (\nSELECT * FROM x\n) AS t", ";WITH x AS (SELECT * FROM T) SELECT * FROM x ORDER BY a\nLIMIT {1} OFFSET {2}", DisplayName = $"PageableSql {DatabaseTypes.Sqlite} ;WITH")]
    public void BuildCountSqlAndBuildPagedSql_PageableSql_BuildsCountAndPagedSql(string databaseType, string sql, string expectedCountSql, string expectedPagedSql)
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
    /// Test SqlDialect.BuildPagedSql with SQL Server TOP queries, which can't be paged because SQL Server doesn't allow TOP and OFFSET / FETCH together.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    [TestMethod]
    [DataRow("SELECT TOP 5 * FROM T ORDER BY a", DisplayName = "SqlServerTop TOP")]
    [DataRow("SELECT ALL TOP (5) * FROM T", DisplayName = "SqlServerTop ALL TOP")]
    [DataRow("SELECT TOP (5) WITH TIES * FROM T ORDER BY a", DisplayName = "SqlServerTop TOP WITH TIES")]
    public void BuildCountSqlAndBuildPagedSql_SqlServerTop_ThrowsNotSupportedException(string sql)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NullShape {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NullShape {DatabaseTypes.SqlServer}")]
    public void BuildCountSqlAndBuildPagedSql_NullShape_ThrowsArgumentNullException(string databaseType)
    {
        // ARRANGE
        SqlDialect dialect = SqlDialect.For(databaseType);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => dialect.BuildCountSql(null!), "Unexpected exception (BuildCountSql)");
        Assert.ThrowsExactly<ArgumentNullException>(() => dialect.BuildPagedSql(null!, 1, 2), "Unexpected exception (BuildPagedSql)");
    }

    #endregion BuildCountSql and BuildPagedSql tests

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
