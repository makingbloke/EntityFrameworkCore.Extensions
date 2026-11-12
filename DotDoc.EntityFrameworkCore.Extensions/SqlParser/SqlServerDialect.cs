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

        if (FindTop(shape, out _))
        {
            throw new NotSupportedException("A query using TOP can't be paged, because SQL Server doesn't allow TOP and OFFSET / FETCH in the same query.");
        }

        // SQL Server must have "OFFSET x ROWS FETCH NEXT y ROWS ONLY" straight after the outer ORDER BY (before any OPTION clause).
        // If there is no ORDER BY then add a dummy one. Each part goes on a new line so a -- comment can't hide it.
        StringBuilder sql = new(shape.Ordered);

        if (shape.OrderByStart is null)
        {
            sql.AppendLine().Append("ORDER BY (SELECT NULL)");
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
        if (afterOrderBy && (token.IsWord("OFFSET") || token.IsWord("FETCH")))
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

    /// <inheritdoc/>
    /// <remarks>TOP n WITH TIES needs its ORDER BY, and the ties change the number of rows. Otherwise TOP returns the same number of rows whatever the order.</remarks>
    protected override bool KeepOrderByInCount(SqlQueryShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        return shape.OrderByStart is not null && FindTop(shape, out bool withTies) && withTies;
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Check if the main query uses TOP: SELECT [ALL | DISTINCT] TOP (n) [PERCENT] [WITH TIES].
    /// </summary>
    /// <param name="shape">The parts of the query.</param>
    /// <param name="withTies">A value indicating whether WITH TIES is used (out).</param>
    /// <returns><see langword="true"/> if the query uses TOP, otherwise <see langword="false"/>.</returns>
    private static bool FindTop(SqlQueryShape shape, out bool withTies)
    {
        IReadOnlyList<SqlToken> tokens = shape.Tokens;
        int i = shape.QueryTokenIndex;
        withTies = false;

        if (!tokens[i].IsWord("SELECT"))
        {
            return false;
        }

        i++;

        if (i < tokens.Count && (tokens[i].IsWord("ALL") || tokens[i].IsWord("DISTINCT")))
        {
            i++;
        }

        if (i >= tokens.Count || !tokens[i].IsWord("TOP"))
        {
            return false;
        }

        // Skip the TOP value: either a number, or an expression in brackets.
        i++;

        if (i < tokens.Count && tokens[i].Type == SqlTokenType.OpenBracket)
        {
            int depth = tokens[i].Depth;
            do
            {
                i++;
            }
            while (i < tokens.Count && !(tokens[i].Type == SqlTokenType.CloseBracket && tokens[i].Depth == depth));
        }

        i++;

        if (i < tokens.Count && tokens[i].IsWord("PERCENT"))
        {
            i++;
        }

        withTies = i + 1 < tokens.Count && tokens[i].IsWord("WITH") && tokens[i + 1].IsWord("TIES");
        return true;
    }

    #endregion private methods
}
