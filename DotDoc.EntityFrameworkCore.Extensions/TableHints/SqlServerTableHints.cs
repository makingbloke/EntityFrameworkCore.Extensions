// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.TableHints;

/// <summary>
/// SQL Server Table Hints (used to override default query optimiser behaviour).
/// </summary>
public sealed class SqlServerTableHints : ITableHint
{
    #region public fields

    /// <summary>
    /// NOEXPAND.
    /// </summary>
    public static readonly SqlServerTableHints NoExpand = new("NOEXPAND");

    /// <summary>
    /// FORCESCAN.
    /// </summary>
    public static readonly SqlServerTableHints ForceScan = new("FORCESCAN");

    /// <summary>
    /// HOLDLOCK.
    /// </summary>
    public static readonly SqlServerTableHints HoldLock = new("HOLDLOCK");

    /// <summary>
    /// NOLOCK.
    /// </summary>
    public static readonly SqlServerTableHints NoLock = new("NOLOCK");

    /// <summary>
    /// NOWAIT.
    /// </summary>
    public static readonly SqlServerTableHints NoWait = new("NOWAIT");

    /// <summary>
    /// PAGLOCK.
    /// </summary>
    public static readonly SqlServerTableHints PagLock = new("PAGLOCK");

    /// <summary>
    /// READCOMMITTED.
    /// </summary>
    public static readonly SqlServerTableHints ReadCommitted = new("READCOMMITTED");

    /// <summary>
    /// READCOMMITTEDLOCK.
    /// </summary>
    public static readonly SqlServerTableHints ReadCommittedLock = new("READCOMMITTEDLOCK");

    /// <summary>
    /// READPAST.
    /// </summary>
    public static readonly SqlServerTableHints ReadPast = new("READPAST");

    /// <summary>
    /// READUNCOMMITTED.
    /// </summary>
    public static readonly SqlServerTableHints ReadUncommitted = new("READUNCOMMITTED");

    /// <summary>
    /// REPEATABLEREAD.
    /// </summary>
    public static readonly SqlServerTableHints RepeatableRead = new("REPEATABLEREAD");

    /// <summary>
    /// ROWLOCK.
    /// </summary>
    public static readonly SqlServerTableHints RowLock = new("ROWLOCK");

    /// <summary>
    /// SERIALIZABLE.
    /// </summary>
    public static readonly SqlServerTableHints Serializable = new("SERIALIZABLE");

    /// <summary>
    /// SNAPSHOT.
    /// </summary>
    public static readonly SqlServerTableHints Snapshot = new("SNAPSHOT");

    /// <summary>
    /// TABLOCK.
    /// </summary>
    public static readonly SqlServerTableHints TabLock = new("TABLOCK");

    /// <summary>
    /// TABLOCKX.
    /// </summary>
    public static readonly SqlServerTableHints TabLockX = new("TABLOCKX");

    /// <summary>
    /// UPDLOCK.
    /// </summary>
    public static readonly SqlServerTableHints Updlock = new("UPDLOCK");

    /// <summary>
    /// XLOCK.
    /// </summary>
    public static readonly SqlServerTableHints XLock = new("XLOCK");

    #endregion public fields

    #region private fields

    /// <summary>
    /// Hint value.
    /// </summary>
    private readonly string _hint;

    #endregion private fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerTableHints"/> class.
    /// </summary>
    /// <param name="hint">The hint.</param>
    private SqlServerTableHints(string hint)
    {
        ArgumentException.ThrowIfNullOrEmpty(hint);

        this._hint = hint;
    }

    #endregion private constructors

    #region public methods

    /// <summary>
    /// INDEX( &lt;index_value&gt; [ , ...n] ) | INDEX = ( &lt;index_value&gt; ).
    /// </summary>
    /// <param name="indexValues">The index values (names).</param>
    /// <returns><see cref="SqlServerTableHints"/>.</returns>
    public static SqlServerTableHints Index(params IEnumerable<string> indexValues)
    {
        if (indexValues is null || !indexValues.Any())
        {
            throw new ArgumentNullException(nameof(indexValues), "At least one index value must be supplied");
        }

        if (indexValues.Any(iv => string.IsNullOrEmpty(iv)))
        {
            throw new ArgumentException("Null or empty index values are not supported", nameof(indexValues));
        }

        string indexes = CreateDelimitedIdentifiers(indexValues);
        return new($"INDEX({indexes})");
    }

    /// <summary>
    /// FORCESEEK[( &lt; index_value &gt; ( &lt; index_column_name &gt; [ , ... ]))].
    /// </summary>
    /// <returns><see cref="SqlServerTableHints"/>.</returns>
    public static SqlServerTableHints ForceSeek()
    {
        return new("FORCESEEK");
    }

    /// <summary>
    /// FORCESEEK[( &lt; index_value &gt; ( &lt; index_column_name &gt; [ , ... ]))].
    /// </summary>
    /// <param name="indexValue">The index value (name).</param>
    /// <param name="indexColumnNames">The index column names.</param>
    /// <returns><see cref="SqlServerTableHints"/>.</returns>
    public static SqlServerTableHints ForceSeek(string indexValue, params IEnumerable<string> indexColumnNames)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexValue);

        if (indexColumnNames is null || !indexColumnNames.Any())
        {
            throw new ArgumentNullException(nameof(indexColumnNames), "At least one index column name must be supplied");
        }

        if (indexColumnNames.Any(icn => string.IsNullOrEmpty(icn)))
        {
            throw new ArgumentException("Null or empty index column names are not supported", nameof(indexColumnNames));
        }

        string index = CreateDelimitedIdentifer(indexValue);
        string columnNames = CreateDelimitedIdentifiers(indexColumnNames);

        return new($"FORCESEEK({index} ({columnNames}))");
    }

    /// <summary>
    /// SPATIAL_WINDOW_MAX_CELLS = &lt;integer_value&gt;.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    /// <returns><see cref="SqlServerTableHints"/>.</returns>
    public static SqlServerTableHints SpacialWindowMaxCells(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 8192);

        return new SqlServerTableHints($"SPATIAL_WINDOW_MAX_CELLS = {value})");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this._hint;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Create a string containing a comma separated list of delimited identifiers.
    /// </summary>
    /// <param name="identifiers">The identifiers.</param>
    /// <returns>The comma separated list of identifiers.</returns>
    private static string CreateDelimitedIdentifiers(IEnumerable<string> identifiers)
    {
        string delimitedIdentifiers = string.Join(", ", identifiers.Select(CreateDelimitedIdentifer).Distinct(StringComparer.OrdinalIgnoreCase));
        return delimitedIdentifiers;
    }

    /// <summary>
    /// Create a delimited identifier.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <returns>The delimited identifier.</returns>
    private static string CreateDelimitedIdentifer(string identifier)
    {
        string delimitedIdentifier = $"[{identifier}]";
        return delimitedIdentifier;
    }

    #endregion private methods
}
