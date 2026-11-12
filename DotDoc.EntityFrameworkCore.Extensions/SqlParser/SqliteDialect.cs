// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// SQLite SQL rules.
/// </summary>
internal sealed class SqliteDialect : SqlDialect
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this dialect.
    /// </summary>
    internal static readonly SqliteDialect Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDialect"/> class.
    /// </summary>
    private SqliteDialect()
    {
    }

    #endregion private constructors

    #region protected properties

    /// <inheritdoc/>
    /// <remarks>SQLite accepts "identifier", [identifier] and `identifier` (https://sqlite.org/lang_keywords.html).</remarks>
    protected override IReadOnlyList<SqlQuote> Quotes { get; } =
        [
            new('\'', '\'', SqlTokenType.String, true),
            new('"', '"', SqlTokenType.QuotedIdentifier, true),
            new('[', ']', SqlTokenType.QuotedIdentifier, false),
            new('`', '`', SqlTokenType.QuotedIdentifier, true)
        ];

    /// <inheritdoc/>
    protected override bool NestedBlockComments => false;

    /// <inheritdoc/>
    /// <remarks>A SQLite block comment runs to the next */ or the end of the SQL, whichever comes first (https://sqlite.org/lang_comment.html).</remarks>
    protected override bool UnclosedBlockComments => true;

    #endregion protected properties

    #region public methods

    /// <inheritdoc/>
    public override string BuildPagedSql(SqlQueryShape shape, int limitParamPos, int offsetParamPos)
    {
        ArgumentNullException.ThrowIfNull(shape);

        // SQLite just needs "LIMIT x OFFSET y" at the end of the query (on a new line so a trailing -- comment can't hide it).
        return $"{shape.Ordered}{Environment.NewLine}LIMIT {{{limitParamPos}}} OFFSET {{{offsetParamPos}}}";
    }

    #endregion public methods

    #region protected methods

    /// <inheritdoc/>
    protected override SqlTrailingClause GetTrailingClause(IReadOnlyList<SqlToken> tokens, int index, bool afterOrderBy)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        return tokens[index].IsWord("LIMIT")
            ? SqlTrailingClause.Paging
            : SqlTrailingClause.None;
    }

    #endregion protected methods
}
