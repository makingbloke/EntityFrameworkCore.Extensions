// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// The SQL rules for a database type, used to split a SELECT statement into its parts and build count and paged versions of it.
/// </summary>
/// <remarks>
/// <para>
/// This is not a full SQL parser. It splits the SQL into tokens (skipping whitespace, comments, strings and quoted identifiers)
/// and tracks the bracket depth, which is enough to find the top level clauses reliably.
/// </para>
/// <para>
/// To support a new database, derive a class from this one, override the members that differ (quotes, comments, trailing
/// clauses and how a query is paged) and add it to <see cref="For(string?)"/>.
/// </para>
/// </remarks>
internal abstract class SqlDialect
{
    #region public properties

    /// <summary>
    /// Gets the alias given to the original query when it is wrapped to count the rows.
    /// </summary>
    public virtual string CountAlias => "t";

    #endregion public properties

    #region protected properties

    /// <summary>
    /// Gets the quoted tokens (strings and quoted identifiers) supported by the database.
    /// </summary>
    protected abstract IReadOnlyList<SqlQuote> Quotes { get; }

    /// <summary>
    /// Gets a value indicating whether block comments can be nested, e.g. /* a /* b */ c */.
    /// </summary>
    protected abstract bool NestedBlockComments { get; }

    /// <summary>
    /// Gets a value indicating whether a block comment can be left unclosed, in which case it runs to the end of the SQL.
    /// </summary>
    protected abstract bool UnclosedBlockComments { get; }

    #endregion protected properties

    #region public methods

    /// <summary>
    /// Get the <see cref="SqlDialect"/> for a database type.
    /// </summary>
    /// <param name="databaseType">The database type (one of the <see cref="DatabaseTypes"/> values).</param>
    /// <returns>The <see cref="SqlDialect"/>.</returns>
    /// <exception cref="UnsupportedDatabaseTypeException">The database type is not supported.</exception>
    public static SqlDialect For(string? databaseType)
    {
        return databaseType switch
        {
            DatabaseTypes.Sqlite => SqliteDialect.Instance,
            DatabaseTypes.SqlServer => SqlServerDialect.Instance,
            _ => throw new UnsupportedDatabaseTypeException(nameof(databaseType))
        };
    }

    /// <summary>
    /// Split SQL into tokens. Whitespace and comments are skipped.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <returns>The tokens, in the order they appear in the SQL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sql"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The SQL has an unclosed quote, comment or bracket, or an unexpected closing bracket.</exception>
    public IReadOnlyList<SqlToken> Tokenize(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        List<SqlToken> tokens = [];
        int depth = 0;
        int i = 0;

        while (i < sql.Length)
        {
            char c = sql[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (c == '-' && CharAt(sql, i + 1) == '-')
            {
                i = SkipLineComment(sql, i);
            }
            else if (c == '/' && CharAt(sql, i + 1) == '*')
            {
                i = this.SkipBlockComment(sql, i);
            }
            else if (c == '(')
            {
                tokens.Add(new(SqlTokenType.OpenBracket, "(", i, depth));
                depth++;
                i++;
            }
            else if (c == ')')
            {
                depth--;

                if (depth < 0)
                {
                    throw new ArgumentException($"The SQL has an unexpected ')' at position {i}.", nameof(sql));
                }

                tokens.Add(new(SqlTokenType.CloseBracket, ")", i, depth));
                i++;
            }
            else
            {
                // Let the dialect read any tokens only it supports (e.g. PostgreSQL dollar quoted strings) first.
                if (!this.TryReadDialectToken(sql, i, out int end, out SqlTokenType type))
                {
                    (end, type) = this.ReadToken(sql, i);
                }

                tokens.Add(new(type, sql[i..end], i, depth));
                i = end;
            }
        }

        if (depth != 0)
        {
            throw new ArgumentException("The SQL has an unclosed '('.", nameof(sql));
        }

        return tokens;
    }

    /// <summary>
    /// Split a SELECT statement into its parts.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <returns>A <see cref="SqlQueryShape"/> describing the parts of the statement.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sql"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The SQL is empty, is more than one statement, or has unclosed quotes, comments or brackets.</exception>
    /// <exception cref="NotSupportedException">The SQL is not a SELECT statement, is already paged, or has a clause that can't be paged.</exception>
    public SqlQueryShape Analyse(string sql)
    {
        IReadOnlyList<SqlToken> tokens = this.Tokenize(sql);

        // Ignore trailing semicolons (trailing comments are already skipped). Anything after another semicolon is a second statement.
        int last = tokens.Count - 1;
        while (last >= 0 && tokens[last].Type == SqlTokenType.Semicolon)
        {
            last--;
        }

        if (last < 0)
        {
            throw new ArgumentException("The SQL does not contain a query.", nameof(sql));
        }

        for (int i = 0; i < last; i++)
        {
            if (tokens[i].Type == SqlTokenType.Semicolon)
            {
                throw new ArgumentException("The SQL must contain a single statement.", nameof(sql));
            }
        }

        int queryIndex = FindQueryStart(tokens, last);

        // The outer ORDER BY is the last ORDER BY at the top level - ORDER BYs in subqueries, window functions etc. are inside brackets.
        int orderByIndex = -1;
        for (int i = last - 1; i > queryIndex; i--)
        {
            if (tokens[i].Depth == 0 && tokens[i].IsWord("ORDER") && tokens[i + 1].IsWord("BY"))
            {
                orderByIndex = i;
                break;
            }
        }

        // Trailing clauses (e.g. LIMIT or OPTION) can only come after the ORDER BY, if there is one.
        int tailIndex = -1;
        for (int i = orderByIndex >= 0 ? orderByIndex + 2 : queryIndex + 1; i <= last; i++)
        {
            if (tokens[i].Depth != 0)
            {
                continue;
            }

            SqlTrailingClause clause = this.GetTrailingClause(tokens, i, orderByIndex >= 0);

            if (clause == SqlTrailingClause.Paging)
            {
                throw new NotSupportedException($"The query is already paged ('{tokens[i].Text}' clause), so it can't be paged again.");
            }

            if (clause == SqlTrailingClause.Unsupported)
            {
                throw new NotSupportedException($"A query with a '{tokens[i].Text} {TokenTextAt(tokens, i + 1)}' clause can't be paged.");
            }

            if (clause == SqlTrailingClause.Hint)
            {
                tailIndex = i;
                break;
            }
        }

        // Work out where each part ends, using the end of the last token in it, so whitespace and comments between parts are left out.
        int afterOrderByIndex = tailIndex >= 0 ? tailIndex : last + 1;
        int afterQueryIndex = orderByIndex >= 0 ? orderByIndex : afterOrderByIndex;

        return new SqlQueryShape(
            sql,
            tokens,
            queryIndex,
            queryEnd: tokens[afterQueryIndex - 1].End,
            orderByStart: orderByIndex >= 0 ? tokens[orderByIndex].Start : null,
            orderByEnd: orderByIndex >= 0 ? tokens[afterOrderByIndex - 1].End : null,
            tailStart: tailIndex >= 0 ? tokens[tailIndex].Start : null,
            contentEnd: tokens[last].End);
    }

    /// <summary>
    /// Build a query that counts the rows returned by the original query.
    /// </summary>
    /// <param name="shape">The parts of the original query.</param>
    /// <returns>The count query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="shape"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The query is wrapped rather than having its column list replaced, so DISTINCT, GROUP BY and UNION queries are counted correctly.
    /// The database merges the wrapped query into the count, so the rows are not returned. The outer ORDER BY is removed because
    /// it isn't needed for a count (and some databases would sort every row before counting them). The trailing clause is left
    /// out - override this method if the database needs it (e.g. SQL Server OPTION).
    /// </remarks>
    public virtual string BuildCountSql(SqlQueryShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        string query = this.KeepOrderByInCount(shape)
            ? shape.Sql[shape.QueryStart..shape.OrderByEnd!.Value]
            : shape.Query;

        // The line breaks stop a trailing -- comment from hiding the closing bracket.
        return $"{shape.Prefix}SELECT COUNT(*) FROM ({Environment.NewLine}{query}{Environment.NewLine}) AS {this.CountAlias}";
    }

    /// <summary>
    /// Build a query that returns one page of the rows returned by the original query.
    /// </summary>
    /// <param name="shape">The parts of the original query.</param>
    /// <param name="limitParamPos">Position of the limit (page size) value in the parameters collection.</param>
    /// <param name="offsetParamPos">Position of the offset value in the parameters collection.</param>
    /// <returns>The paged query, with {limitParamPos} and {offsetParamPos} placeholders for the page size and offset.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="shape"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">The query can't be paged by this database (e.g. SQL Server TOP queries).</exception>
    public abstract string BuildPagedSql(SqlQueryShape shape, int limitParamPos, int offsetParamPos);

    #endregion public methods

    #region protected methods

    /// <summary>
    /// Check if a top level token starts a trailing clause (e.g. LIMIT or OPTION).
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <param name="index">The index of the token to check.</param>
    /// <param name="afterOrderBy">A value indicating whether the token is after the outer ORDER BY.</param>
    /// <returns>The type of clause the token starts.</returns>
    /// <remarks>Use the next token to avoid treating an unquoted column name (e.g. Option) as a keyword.</remarks>
    protected abstract SqlTrailingClause GetTrailingClause(IReadOnlyList<SqlToken> tokens, int index, bool afterOrderBy);

    /// <summary>
    /// Check if the outer ORDER BY must be kept in the count query.
    /// </summary>
    /// <param name="shape">The parts of the original query.</param>
    /// <returns><see langword="true"/> to keep the ORDER BY (e.g. SQL Server TOP ... WITH TIES needs it), otherwise <see langword="false"/>.</returns>
    protected virtual bool KeepOrderByInCount(SqlQueryShape shape)
    {
        return false;
    }

    /// <summary>
    /// Read a token that only this database supports (e.g. PostgreSQL dollar quoted strings).
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position of the token.</param>
    /// <param name="end">The position immediately after the token (out).</param>
    /// <param name="type">The token type (out).</param>
    /// <returns><see langword="true"/> if a token was read, otherwise <see langword="false"/>.</returns>
    protected virtual bool TryReadDialectToken(string sql, int start, out int end, out SqlTokenType type)
    {
        end = start;
        type = SqlTokenType.Symbol;
        return false;
    }

    /// <summary>
    /// Check if a character can start an unquoted word (keyword, identifier or variable).
    /// </summary>
    /// <param name="c">The character.</param>
    /// <returns><see langword="true"/> if it can, otherwise <see langword="false"/>.</returns>
    protected virtual bool IsWordStart(char c)
    {
        return char.IsLetter(c) || c == '_' || c == '@' || c == '#';
    }

    /// <summary>
    /// Check if a character can be part of an unquoted word (keyword, identifier or variable).
    /// </summary>
    /// <param name="c">The character.</param>
    /// <returns><see langword="true"/> if it can, otherwise <see langword="false"/>.</returns>
    protected virtual bool IsWordPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '@' || c == '#' || c == '$';
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Get the character at a position, or '\0' if the position is past the end of the SQL.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="index">The position.</param>
    /// <returns>The character.</returns>
    private static char CharAt(string sql, int index)
    {
        return index < sql.Length ? sql[index] : '\0';
    }

    /// <summary>
    /// Get the text of a token, or an empty string if the index is past the last token.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <param name="index">The token index.</param>
    /// <returns>The token text.</returns>
    private static string TokenTextAt(IReadOnlyList<SqlToken> tokens, int index)
    {
        return index < tokens.Count ? tokens[index].Text : string.Empty;
    }

    /// <summary>
    /// Find the first token of the main query, skipping any WITH clause (common table expressions).
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <param name="last">The index of the last token in the statement.</param>
    /// <returns>The index of the first token of the main query.</returns>
    private static int FindQueryStart(IReadOnlyList<SqlToken> tokens, int last)
    {
        if (tokens[0].IsWord("SELECT") || tokens[0].Type == SqlTokenType.OpenBracket)
        {
            return 0;
        }

        if (tokens[0].IsWord("WITH"))
        {
            // The common table expressions are inside brackets, so the main query is the first top level SELECT.
            for (int i = 1; i <= last; i++)
            {
                if (tokens[i].Depth == 0 && tokens[i].IsWord("SELECT"))
                {
                    return i;
                }
            }
        }

        throw new NotSupportedException("Only SELECT queries can be paged.");
    }

    /// <summary>
    /// Read a quoted token, allowing for escaped quotes.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position of the opening quote.</param>
    /// <param name="quote">The quote rules.</param>
    /// <returns>The position immediately after the closing quote.</returns>
    private static int ReadQuoted(string sql, int start, SqlQuote quote)
    {
        int i = start + 1;

        while (i < sql.Length)
        {
            if (sql[i] == quote.Close)
            {
                if (quote.DoubledCloseIsEscape && CharAt(sql, i + 1) == quote.Close)
                {
                    i += 2;
                    continue;
                }

                return i + 1;
            }

            i++;
        }

        throw new ArgumentException($"The SQL has an unclosed {quote.Open} at position {start}.", nameof(sql));
    }

    /// <summary>
    /// Read characters while they match a condition.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position to start reading from.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>The position of the first character that doesn't match.</returns>
    private static int ReadWhile(string sql, int start, Func<char, bool> condition)
    {
        int i = start;

        while (i < sql.Length && condition(sql[i]))
        {
            i++;
        }

        return i;
    }

    /// <summary>
    /// Skip a -- line comment.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position of the comment.</param>
    /// <returns>The position immediately after the comment.</returns>
    private static int SkipLineComment(string sql, int start)
    {
        int end = sql.IndexOf('\n', start);
        return end < 0 ? sql.Length : end + 1;
    }

    /// <summary>
    /// Read a quoted token, semicolon, word, number or symbol.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position of the token.</param>
    /// <returns>The position immediately after the token, and the token type.</returns>
    private (int End, SqlTokenType Type) ReadToken(string sql, int start)
    {
        char c = sql[start];

        if (this.Quotes.FirstOrDefault(q => q.Open == c) is SqlQuote quote)
        {
            return (ReadQuoted(sql, start, quote), quote.Type);
        }

        if (c == ';')
        {
            return (start + 1, SqlTokenType.Semicolon);
        }

        if (this.IsWordStart(c))
        {
            return (ReadWhile(sql, start + 1, this.IsWordPart), SqlTokenType.Word);
        }

        if (char.IsAsciiDigit(c))
        {
            return (ReadWhile(sql, start + 1, ch => char.IsAsciiLetterOrDigit(ch) || ch == '.'), SqlTokenType.Number);
        }

        return (start + 1, SqlTokenType.Symbol);
    }

    /// <summary>
    /// Skip a /* block */ comment.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="start">The position of the comment.</param>
    /// <returns>The position immediately after the comment (the end of the SQL if the comment is unclosed and the database allows it).</returns>
    private int SkipBlockComment(string sql, int start)
    {
        int nesting = 0;
        int i = start;

        while (i < sql.Length - 1)
        {
            if (sql[i] == '/' && sql[i + 1] == '*')
            {
                if (nesting == 0 || this.NestedBlockComments)
                {
                    nesting++;
                }

                i += 2;
            }
            else if (sql[i] == '*' && sql[i + 1] == '/')
            {
                nesting--;
                i += 2;

                if (nesting == 0)
                {
                    return i;
                }
            }
            else
            {
                i++;
            }
        }

        // Nothing after an unclosed comment is a token, so any SQL appended to the parts of the query can't end up inside it.
        return this.UnclosedBlockComments
            ? sql.Length
            : throw new ArgumentException($"The SQL has an unclosed /* comment at position {start}.", nameof(sql));
    }

    #endregion private methods
}
