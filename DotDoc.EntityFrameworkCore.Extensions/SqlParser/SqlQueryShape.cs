// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// The parts of a SELECT statement, found by <see cref="SqlDialect.Analyse(string)"/>.
/// </summary>
/// <remarks>
/// The parts are positions in the original SQL, so it can be rebuilt without changing the user's formatting or parameter placeholders:
/// <code>
/// [Prefix]           [Query]                                   [OrderBy]              [Tail]               [ignored]
/// WITH x AS (...)    SELECT ... FROM ... WHERE ... GROUP BY    ORDER BY Name DESC     OPTION (RECOMPILE)   ; -- comment
/// </code>
/// </remarks>
internal sealed class SqlQueryShape
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlQueryShape"/> class.
    /// </summary>
    /// <param name="sql">The original SQL.</param>
    /// <param name="tokens">The SQL tokens.</param>
    /// <param name="queryTokenIndex">The index of the first token of the main query (after any WITH clause).</param>
    /// <param name="queryEnd">The position immediately after the main query.</param>
    /// <param name="orderByStart">The position of the outer ORDER BY clause, or <see langword="null"/> if there isn't one.</param>
    /// <param name="orderByEnd">The position immediately after the outer ORDER BY clause, or <see langword="null"/> if there isn't one.</param>
    /// <param name="tailStart">The position of the trailing clause (e.g. OPTION), or <see langword="null"/> if there isn't one.</param>
    /// <param name="contentEnd">The position immediately after the last token, ignoring trailing semicolons and comments.</param>
    public SqlQueryShape(string sql, IReadOnlyList<SqlToken> tokens, int queryTokenIndex, int queryEnd, int? orderByStart, int? orderByEnd, int? tailStart, int contentEnd)
    {
        this.Sql = sql;
        this.Tokens = tokens;
        this.QueryTokenIndex = queryTokenIndex;
        this.QueryEnd = queryEnd;
        this.OrderByStart = orderByStart;
        this.OrderByEnd = orderByEnd;
        this.TailStart = tailStart;
        this.ContentEnd = contentEnd;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the original SQL.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Gets the SQL tokens.
    /// </summary>
    public IReadOnlyList<SqlToken> Tokens { get; }

    /// <summary>
    /// Gets the index in <see cref="Tokens"/> of the first token of the main query (after any WITH clause).
    /// </summary>
    public int QueryTokenIndex { get; }

    /// <summary>
    /// Gets the position of the main query (after any WITH clause).
    /// </summary>
    public int QueryStart => this.Tokens[this.QueryTokenIndex].Start;

    /// <summary>
    /// Gets the position immediately after the main query (before any ORDER BY or trailing clause).
    /// </summary>
    public int QueryEnd { get; }

    /// <summary>
    /// Gets the position of the outer ORDER BY clause, or <see langword="null"/> if there isn't one.
    /// </summary>
    public int? OrderByStart { get; }

    /// <summary>
    /// Gets the position immediately after the outer ORDER BY clause, or <see langword="null"/> if there isn't one.
    /// </summary>
    public int? OrderByEnd { get; }

    /// <summary>
    /// Gets the position of the trailing clause (e.g. OPTION (...)), or <see langword="null"/> if there isn't one.
    /// </summary>
    public int? TailStart { get; }

    /// <summary>
    /// Gets the position immediately after the last token, ignoring trailing semicolons and comments.
    /// </summary>
    public int ContentEnd { get; }

    /// <summary>
    /// Gets everything before the main query (e.g. a WITH clause), or an empty string.
    /// </summary>
    public string Prefix => this.Sql[..this.QueryStart];

    /// <summary>
    /// Gets the main query, without any outer ORDER BY or trailing clause.
    /// </summary>
    public string Query => this.Sql[this.QueryStart..this.QueryEnd];

    /// <summary>
    /// Gets the SQL up to the end of the outer ORDER BY (or the main query if there isn't one), including any prefix.
    /// </summary>
    public string Ordered => this.Sql[..(this.OrderByEnd ?? this.QueryEnd)];

    /// <summary>
    /// Gets the trailing clause (e.g. OPTION (...)), or <see langword="null"/> if there isn't one.
    /// </summary>
    public string? Tail => this.TailStart is int tailStart ? this.Sql[tailStart..this.ContentEnd] : null;

    #endregion public properties
}
