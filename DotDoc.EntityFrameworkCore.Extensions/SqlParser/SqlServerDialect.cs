// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Globalization;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// SQL Server SQL rules.
/// </summary>
internal sealed class SqlServerDialect : SqlDialect
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this dialect.
    /// </summary>
    internal static readonly SqlServerDialect Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDialect"/> class.
    /// </summary>
    private SqlServerDialect()
    {
    }

    #endregion private constructors

    #region protected properties

    /// <inheritdoc/>
    /// <remarks>"identifier" assumes QUOTED_IDENTIFIER is ON, which is the default (and is required by EF Core).</remarks>
    protected override IReadOnlyList<SqlQuote> Quotes { get; } =
        [
            new('\'', '\'', SqlTokenType.String, true),
            new('[', ']', SqlTokenType.QuotedIdentifier, true),
            new('"', '"', SqlTokenType.QuotedIdentifier, true)
        ];

    /// <inheritdoc/>
    protected override bool NestedBlockComments => true;

    /// <inheritdoc/>
    /// <remarks>SQL Server rejects an unclosed block comment ("Missing end comment mark '*/'").</remarks>
    protected override bool UnclosedBlockComments => false;

    #endregion protected properties

    #region public methods

    /// <inheritdoc/>
    /// <remarks>A trailing OPTION (...) clause is kept, after the wrapped query.</remarks>
    public override string BuildCountSql(SqlQueryShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        string countSql = base.BuildCountSql(shape);

        return shape.Tail is null
            ? countSql
            : $"{countSql}{Environment.NewLine}{shape.Tail}";
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">The query uses TOP, which SQL Server doesn't allow with OFFSET / FETCH.</exception>
    public override string BuildPagedSql(SqlQueryShape shape, int limitParamPos, int offsetParamPos)
    {
        ArgumentNullException.ThrowIfNull(shape);

        if (FindTop(shape))
        {
            throw new NotSupportedException("A query using TOP can't be paged, because SQL Server doesn't allow TOP and OFFSET / FETCH in the same query.");
        }

        // SQL Server must have "OFFSET x ROWS FETCH NEXT y ROWS ONLY" straight after the outer ORDER BY (before any OPTION clause).
        // If there is no ORDER BY then wrap the query, as the count query does, and add a dummy one to the wrapper, as SQL Server
        // rejects ORDER BY (SELECT NULL) after a UNION, INTERSECT or EXCEPT, or on a SELECT DISTINCT. Each part goes on a new line
        // so a -- comment can't hide it.
        StringBuilder sql = new();

        if (shape.OrderByStart is null)
        {
            sql.Append(shape.Prefix)
                .AppendLine("SELECT * FROM (")
                .AppendLine(shape.Query)
                .AppendLine(CultureInfo.InvariantCulture, $") AS {this.CountAlias}")
                .Append("ORDER BY (SELECT NULL)");
        }
        else
        {
            sql.Append(shape.Ordered);
        }

        sql.AppendLine().Append(CultureInfo.InvariantCulture, $"OFFSET {{{offsetParamPos}}} ROWS FETCH NEXT {{{limitParamPos}}} ROWS ONLY");

        if (shape.Tail is not null)
        {
            sql.AppendLine().Append(shape.Tail);
        }

        return sql.ToString();
    }

    #endregion public methods

    #region protected methods

    /// <inheritdoc/>
    protected override SqlTrailingClause GetTrailingClause(IReadOnlyList<SqlToken> tokens, int index, bool afterOrderBy)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        SqlToken token = tokens[index];
        SqlToken? next = index + 1 < tokens.Count ? tokens[index + 1] : null;

        // OFFSET / FETCH are only valid after an ORDER BY, which avoids treating a column called Offset as a keyword.
        // OFFSET isn't a reserved word, so a column called Offset can also be in the ORDER BY list (FETCH is reserved, so it can't).
        if (afterOrderBy && (token.IsWord("FETCH") || (token.IsWord("OFFSET") && IsOffsetClause(tokens, index))))
        {
            return SqlTrailingClause.Paging;
        }

        if (token.IsWord("OPTION") && next?.Type == SqlTokenType.OpenBracket)
        {
            return SqlTrailingClause.Hint;
        }

        if (token.IsWord("FOR") && next is { } forType && (forType.IsWord("XML") || forType.IsWord("JSON") || forType.IsWord("BROWSE")))
        {
            return SqlTrailingClause.Unsupported;
        }

        return SqlTrailingClause.None;
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Check if the main query uses TOP: SELECT [ALL | DISTINCT] TOP.
    /// </summary>
    /// <param name="shape">The parts of the query.</param>
    /// <returns><see langword="true"/> if the query uses TOP, otherwise <see langword="false"/>.</returns>
    private static bool FindTop(SqlQueryShape shape)
    {
        IReadOnlyList<SqlToken> tokens = shape.Tokens;
        int i = shape.QueryTokenIndex;

        if (!tokens[i].IsWord("SELECT"))
        {
            return false;
        }

        i++;

        if (i < tokens.Count && (tokens[i].IsWord("ALL") || tokens[i].IsWord("DISTINCT")))
        {
            i++;
        }

        return i < tokens.Count && tokens[i].IsWord("TOP");
    }

    /// <summary>
    /// Check if a top level OFFSET word starts an OFFSET ... ROW / ROWS clause, rather than being a column called Offset in the ORDER BY list.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <param name="offsetIndex">The index of the OFFSET word.</param>
    /// <returns><see langword="true"/> if it starts an OFFSET clause, otherwise <see langword="false"/>.</returns>
    private static bool IsOffsetClause(IReadOnlyList<SqlToken> tokens, int offsetIndex)
    {
        // The offset can be an expression (e.g. OFFSET @first - 1 ROWS), but it can't contain a top level comma, which would start the next
        // ORDER BY item.
        for (int i = offsetIndex + 1; i < tokens.Count; i++)
        {
            SqlToken token = tokens[i];

            if (token.Depth != 0)
            {
                continue;
            }

            if (token.IsWord("ROW") || token.IsWord("ROWS"))
            {
                return true;
            }

            if (token.Type == SqlTokenType.Semicolon || (token.Type == SqlTokenType.Symbol && token.Text == ","))
            {
                return false;
            }
        }

        return false;
    }

    #endregion private methods
}
