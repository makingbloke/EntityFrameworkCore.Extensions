// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Translator.
/// </summary>
/// <remarks>
/// Translates string methods on a SQLite column with the NOCASE collation, or on <c>EF.Functions.Collate(..., "NOCASE")</c>, to the nocase_*
/// functions registered by <see cref="SqliteNoCaseReplacementInterceptor"/>, so they ignore case as the collation does (and as SQL Server does
/// with the collation set by UseCaseInsensitiveCollation). For anything else it returns <see langword="null"/>, so EF Core's own translation is used:
/// a column without the NOCASE collation, an expression with no known collation (such as a concatenation or Substring), or a column from raw SQL,
/// a subquery or a set operation.
/// <list type="bullet">
/// <item><description>EndsWith and StartsWith use nocase_endswith and nocase_startswith (EF Core uses LIKE, which needs the suffix or prefix escaped).</description></item>
/// <item><description>Contains and IndexOf use nocase_instr.</description></item>
/// <item><description>EF.Functions.Like uses nocase_like, when its match expression has the NOCASE collation.</description></item>
/// <item><description>TrimStart, TrimEnd and Trim with constant characters use nocase_ltrim, nocase_rtrim and nocase_trim.</description></item>
/// <item><description>Replace uses nocase_replace.</description></item>
/// </list>
/// Apart from StartsWith, EndsWith and IndexOf(string, int) (which uses the three-argument nocase_instr rather than substr), each translation has the same shape
/// as EF Core's own SQLite translation, with the nocase_* function in place of the built-in one.
/// <para>
/// StartsWith and EndsWith can't be left to EF Core's translation, even on a NOCASE column. SQLite has no startswith or endswith function, so:
/// <list type="bullet">
/// <item><description>For a constant or parameter, EF Core escapes the value and generates <c>x LIKE 'abc%'</c> or <c>x LIKE @p_startswith ESCAPE '\'</c>.
/// LIKE ignores the column's collation (SQLite's like only ignores the case of ASCII letters), so StartsWith("éc") wouldn't match "École".</description></item>
/// <item><description>For a column argument, EF Core generates <c>substr(x, 1, length(y)) = y OR y = ''</c>. The comparison uses the collation of y,
/// not of x (a function result has no collation), so it's case sensitive unless y is a NOCASE column. Even then, substr and length count code points,
/// and a match can be a different length (é against e + a combining accent, or text with ignorable characters such as a soft hyphen),
/// so the prefix can be cut in the wrong place.</description></item>
/// </list>
/// nocase_endswith and nocase_startswith use the culture-aware IsSuffix and IsPrefix, which handle both, and need no LIKE escaping.
/// </para>
/// The collation is read from the design-time model, as the runtime model has no collations (<see cref="RelationalPropertyExtensions.GetCollation(IReadOnlyProperty)"/>
/// throws on a runtime property). EF Core includes the model in its compiled query cache key, so a translation is cached per model.
/// </remarks>
internal sealed class SqliteNoCaseTranslator : IMethodCallTranslator
{
    #region private fields

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Contains(string)"/> method.
    /// </summary>
    private static readonly MethodInfo ContainsStringMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Contains(char)"/> method.
    /// </summary>
    private static readonly MethodInfo ContainsCharMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.EndsWith(string)"/> method.
    /// </summary>
    private static readonly MethodInfo EndsWithStringMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.EndsWith(char)"/> method.
    /// </summary>
    private static readonly MethodInfo EndsWithCharMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.IndexOf(string)"/> method.
    /// </summary>
    private static readonly MethodInfo IndexOfStringMethod = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.IndexOf(char)"/> method.
    /// </summary>
    private static readonly MethodInfo IndexOfCharMethod = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.IndexOf(string, int)"/> method.
    /// </summary>
    private static readonly MethodInfo IndexOfStringWithStartIndexMethod = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(string), typeof(int)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.IndexOf(char, int)"/> method.
    /// </summary>
    private static readonly MethodInfo IndexOfCharWithStartIndexMethod = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(char), typeof(int)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string)"/> method.
    /// </summary>
    private static readonly MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string, string)"/> method.
    /// </summary>
    private static readonly MethodInfo LikeWithEscapeMethod = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Replace(string, string)"/> method.
    /// </summary>
    private static readonly MethodInfo ReplaceStringMethod = typeof(string).GetMethod(nameof(string.Replace), [typeof(string), typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Replace(char, char)"/> method.
    /// </summary>
    private static readonly MethodInfo ReplaceCharMethod = typeof(string).GetMethod(nameof(string.Replace), [typeof(char), typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.StartsWith(string)"/> method.
    /// </summary>
    private static readonly MethodInfo StartsWithStringMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.StartsWith(char)"/> method.
    /// </summary>
    private static readonly MethodInfo StartsWithCharMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.TrimEnd(char)"/> method.
    /// </summary>
    private static readonly MethodInfo TrimEndCharMethod = typeof(string).GetMethod(nameof(string.TrimEnd), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.TrimEnd(char[])"/> method.
    /// </summary>
    private static readonly MethodInfo TrimEndCharArrayMethod = typeof(string).GetMethod(nameof(string.TrimEnd), [typeof(char[])])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Trim(char)"/> method.
    /// </summary>
    private static readonly MethodInfo TrimCharMethod = typeof(string).GetMethod(nameof(string.Trim), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.Trim(char[])"/> method.
    /// </summary>
    private static readonly MethodInfo TrimCharArrayMethod = typeof(string).GetMethod(nameof(string.Trim), [typeof(char[])])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.TrimStart(char)"/> method.
    /// </summary>
    private static readonly MethodInfo TrimStartCharMethod = typeof(string).GetMethod(nameof(string.TrimStart), [typeof(char)])!;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the <see cref="string.TrimStart(char[])"/> method.
    /// </summary>
    private static readonly MethodInfo TrimStartCharArrayMethod = typeof(string).GetMethod(nameof(string.TrimStart), [typeof(char[])])!;

    /// <summary>
    /// The design-time model, which has the collations the runtime model leaves out.
    /// </summary>
    private readonly IDesignTimeModel _designTimeModel;

    /// <summary>
    /// The SQL expression factory.
    /// </summary>
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseTranslator"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <param name="designTimeModel">The design-time model.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> or <paramref name="designTimeModel"/> is <see langword="null"/>.</exception>
    public SqliteNoCaseTranslator(ISqlExpressionFactory sqlExpressionFactory, IDesignTimeModel designTimeModel)
    {
        ArgumentNullException.ThrowIfNull(sqlExpressionFactory);
        ArgumentNullException.ThrowIfNull(designTimeModel);

        this._sqlExpressionFactory = sqlExpressionFactory;
        this._designTimeModel = designTimeModel;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(arguments);

        // The column whose collation decides the translation: the match expression for EF.Functions.Like, otherwise the string the method is called on.
        SqlExpression? column = method == LikeMethod || method == LikeWithEscapeMethod ? arguments[1] : instance;

        if (column is null || !this.IsNoCase(column))
        {
            return null;
        }

        return method switch
        {
            _ when method == ContainsStringMethod || method == ContainsCharMethod => this.TranslateContains(column, arguments[0]),
            _ when method == IndexOfStringMethod || method == IndexOfCharMethod => this.TranslateIndexOf(column, arguments[0]),
            _ when method == IndexOfStringWithStartIndexMethod || method == IndexOfCharWithStartIndexMethod => this.TranslateIndexOfWithStartIndex(column, arguments[0], arguments[1]),
            _ when method == StartsWithStringMethod || method == StartsWithCharMethod => this.TranslateStartsWith(column, arguments[0]),
            _ when method == EndsWithStringMethod || method == EndsWithCharMethod => this.TranslateEndsWith(column, arguments[0]),
            _ when method == ReplaceStringMethod || method == ReplaceCharMethod => this.TranslateReplace(column, arguments[0], arguments[1]),
            _ when method == TrimCharMethod || method == TrimCharArrayMethod => this.TranslateTrim(column, arguments[0]),
            _ when method == TrimStartCharMethod || method == TrimStartCharArrayMethod => this.TranslateTrimStart(column, arguments[0]),
            _ when method == TrimEndCharMethod || method == TrimEndCharArrayMethod => this.TranslateTrimEnd(column, arguments[0]),
            _ when method == LikeMethod || method == LikeWithEscapeMethod => this.TranslateLike(arguments),
            _ => null,
        };
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Gets the characters to trim from the argument of a Trim, TrimStart or TrimEnd method.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <returns>
    /// The characters, or <see langword="null"/> if the argument isn't a constant character or a constant, non-empty array of characters
    /// (EF Core doesn't translate the others either, and an empty array trims white space).
    /// </returns>
    private static string? GetTrimCharacters(SqlExpression argument)
    {
        return argument switch
        {
            SqlConstantExpression { Value: char character } => new string(character, 1),
            SqlConstantExpression { Value: char[] { Length: > 0 } characters } => new string(characters),
            _ => null,
        };
    }

    /// <summary>
    /// Translates <see cref="string.Contains(string)"/> or <see cref="string.Contains(char)"/>.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="search">The string or character to find.</param>
    /// <returns>The translation.</returns>
    /// <remarks><c>nocase_instr(x, s) &gt; 0</c>, the same shape as EF Core's own translation with instr.</remarks>
    private SqlExpression TranslateContains(SqlExpression instance, SqlExpression search)
    {
        return this._sqlExpressionFactory.GreaterThan(this.NoCaseInstr(instance, search), this._sqlExpressionFactory.Constant(0));
    }

    /// <summary>
    /// Translates <see cref="string.EndsWith(string)"/> or <see cref="string.EndsWith(char)"/>.
    /// </summary>
    /// <param name="instance">The string to check.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>The translation.</returns>
    /// <remarks><c>nocase_endswith(x, s)</c>. The class remarks explain why EF Core's own translation can't be kept.</remarks>
    private SqlExpression TranslateEndsWith(SqlExpression instance, SqlExpression suffix)
    {
        return this.BoolFunction(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, instance, suffix);
    }

    /// <summary>
    /// Translates <see cref="string.IndexOf(string)"/> or <see cref="string.IndexOf(char)"/>.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="search">The string or character to find.</param>
    /// <returns>The translation.</returns>
    /// <remarks><c>nocase_instr(x, s) - 1</c>, the same shape as EF Core's own translation with instr.</remarks>
    private SqlExpression TranslateIndexOf(SqlExpression instance, SqlExpression search)
    {
        return this._sqlExpressionFactory.Subtract(this.NoCaseInstr(instance, search), this._sqlExpressionFactory.Constant(1));
    }

    /// <summary>
    /// Translates <see cref="string.IndexOf(string, int)"/> or <see cref="string.IndexOf(char, int)"/>.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="search">The string or character to find.</param>
    /// <param name="startIndex">The zero-based position to start the search from.</param>
    /// <returns>The translation.</returns>
    /// <remarks>
    /// <c>nocase_instr(x, s, start + 1) - 1</c>, using the three-argument nocase_instr, which starts the search at a one-based position.
    /// EF Core's own translation is <c>(instr(substr(x, start + 1), s) - 1) + start</c>, which gives <c>start - 1</c> rather than -1 when there's no match.
    /// </remarks>
    private SqlExpression TranslateIndexOfWithStartIndex(SqlExpression instance, SqlExpression search, SqlExpression startIndex)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, search);

        SqlExpression function = this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.NoCaseInstrFunction,
            [
                this._sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping),
                this.ApplyArgumentTypeMapping(search, typeMapping),
                this._sqlExpressionFactory.Add(startIndex, this._sqlExpressionFactory.Constant(1)),
            ],
            nullable: true,
            argumentsPropagateNullability: [true, true, true],
            typeof(int));

        return this._sqlExpressionFactory.Subtract(function, this._sqlExpressionFactory.Constant(1));
    }

    /// <summary>
    /// Translates <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string)"/> or <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string, string)"/>.
    /// </summary>
    /// <param name="arguments">The method's arguments: the <see cref="DbFunctions"/> instance, the match expression, the pattern and (optionally) the escape character.</param>
    /// <returns>The translation.</returns>
    /// <remarks>nocase_like takes the pattern first, as SQLite's like function does (<c>x LIKE p</c> is <c>like(p, x)</c>).</remarks>
    private SqlExpression TranslateLike(IReadOnlyList<SqlExpression> arguments)
    {
        SqlExpression match = arguments[1];
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(match, arguments[2]);
        List<SqlExpression> functionArguments =
        [
            this._sqlExpressionFactory.ApplyTypeMapping(arguments[2], typeMapping),
            this._sqlExpressionFactory.ApplyTypeMapping(match, typeMapping),
        ];

        if (arguments.Count > 3)
        {
            functionArguments.Add(this._sqlExpressionFactory.ApplyTypeMapping(arguments[3], typeMapping));
        }

        SqlExpression function = this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.NoCaseLikeFunction,
            functionArguments,
            nullable: true,
            argumentsPropagateNullability: functionArguments.Select(_ => true),
            typeof(bool));

        return this._sqlExpressionFactory.ApplyDefaultTypeMapping(function);
    }

    /// <summary>
    /// Translates <see cref="string.Replace(string, string)"/> or <see cref="string.Replace(char, char)"/>.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="oldValue">The string or character to replace.</param>
    /// <param name="newValue">The replacement.</param>
    /// <returns>The translation.</returns>
    private SqlExpression TranslateReplace(SqlExpression instance, SqlExpression oldValue, SqlExpression newValue)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, oldValue, newValue);

        return this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.NoCaseReplaceFunction,
            [
                this._sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping),
                this.ApplyArgumentTypeMapping(oldValue, typeMapping),
                this.ApplyArgumentTypeMapping(newValue, typeMapping),
            ],
            nullable: true,
            argumentsPropagateNullability: [true, true, true],
            typeof(string),
            typeMapping);
    }

    /// <summary>
    /// Translates <see cref="string.StartsWith(string)"/> or <see cref="string.StartsWith(char)"/>.
    /// </summary>
    /// <param name="instance">The string to check.</param>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The translation.</returns>
    /// <remarks><c>nocase_startswith(x, s)</c>. The class remarks explain why EF Core's own translation can't be kept.</remarks>
    private SqlExpression TranslateStartsWith(SqlExpression instance, SqlExpression prefix)
    {
        return this.BoolFunction(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, instance, prefix);
    }

    /// <summary>
    /// Translates <see cref="string.Trim(char)"/> or <see cref="string.Trim(char[])"/>.
    /// </summary>
    /// <param name="instance">The string to trim.</param>
    /// <param name="characters">The characters to trim.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    private SqlExpression? TranslateTrim(SqlExpression instance, SqlExpression characters)
    {
        return this.NoCaseTrim(SqliteUnicodeFunctionNames.NoCaseTrimFunction, instance, characters);
    }

    /// <summary>
    /// Translates <see cref="string.TrimEnd(char)"/> or <see cref="string.TrimEnd(char[])"/>.
    /// </summary>
    /// <param name="instance">The string to trim.</param>
    /// <param name="characters">The characters to trim.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    private SqlExpression? TranslateTrimEnd(SqlExpression instance, SqlExpression characters)
    {
        return this.NoCaseTrim(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, instance, characters);
    }

    /// <summary>
    /// Translates <see cref="string.TrimStart(char)"/> or <see cref="string.TrimStart(char[])"/>.
    /// </summary>
    /// <param name="instance">The string to trim.</param>
    /// <param name="characters">The characters to trim.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    private SqlExpression? TranslateTrimStart(SqlExpression instance, SqlExpression characters)
    {
        return this.NoCaseTrim(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, instance, characters);
    }

    /// <summary>
    /// Creates a call to nocase_instr.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="search">The string or character to find.</param>
    /// <returns>The call.</returns>
    private SqlExpression NoCaseInstr(SqlExpression instance, SqlExpression search)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, search);

        return this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.NoCaseInstrFunction,
            [this._sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping), this.ApplyArgumentTypeMapping(search, typeMapping)],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(int));
    }

    /// <summary>
    /// Creates a call to nocase_trim, nocase_ltrim or nocase_rtrim.
    /// </summary>
    /// <param name="functionName">The nocase_* function to use.</param>
    /// <param name="instance">The string to trim.</param>
    /// <param name="argument">The characters to trim.</param>
    /// <returns>The call, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    private SqlExpression? NoCaseTrim(string functionName, SqlExpression instance, SqlExpression argument)
    {
        string? characters = GetTrimCharacters(argument);

        if (characters is null)
        {
            return null;
        }

        return this._sqlExpressionFactory.Function(
            functionName,
            [instance, this._sqlExpressionFactory.Constant(characters, instance.TypeMapping)],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(string),
            instance.TypeMapping);
    }

    /// <summary>
    /// Creates a call to a nocase_* function that takes two strings and returns a boolean.
    /// </summary>
    /// <param name="functionName">The function.</param>
    /// <param name="instance">The first argument, the string the method is called on.</param>
    /// <param name="argument">The second argument, a string or character.</param>
    /// <returns>The call.</returns>
    private SqlExpression BoolFunction(string functionName, SqlExpression instance, SqlExpression argument)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);

        SqlExpression function = this._sqlExpressionFactory.Function(
            functionName,
            [this._sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping), this.ApplyArgumentTypeMapping(argument, typeMapping)],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(bool));

        return this._sqlExpressionFactory.ApplyDefaultTypeMapping(function);
    }

    /// <summary>
    /// Applies a string type mapping to a method argument, or the default type mapping if the argument is a character.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="typeMapping">The string type mapping.</param>
    /// <returns>The argument with a type mapping.</returns>
    private SqlExpression ApplyArgumentTypeMapping(SqlExpression argument, RelationalTypeMapping? typeMapping)
    {
        return argument.Type == typeof(char)
            ? this._sqlExpressionFactory.ApplyDefaultTypeMapping(argument)
            : this._sqlExpressionFactory.ApplyTypeMapping(argument, typeMapping);
    }

    /// <summary>
    /// Determines whether an expression has the NOCASE collation.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="expression"/> is <c>EF.Functions.Collate(..., "NOCASE")</c> or a table column with the NOCASE collation,
    /// otherwise <see langword="false"/> (including when the collation isn't known).
    /// </returns>
    private bool IsNoCase(SqlExpression expression)
    {
        string? collation = expression switch
        {
            CollateExpression collateExpression => collateExpression.Collation,
            ColumnExpression { Column: { } column } => this.GetCollation(column),
            _ => null,
        };

        return string.Equals(collation, SqliteUnicodeFunctionNames.NoCaseCollation, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the collation of a column from the design-time model.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The collation, or <see langword="null"/> if it has none or it can't be found.</returns>
    /// <remarks>
    /// The collation is read from the property mapped to the column. Only properties of entity types are looked up
    /// (a property of a complex type gives <see langword="null"/>, so EF Core's own translation is used).
    /// </remarks>
    private string? GetCollation(IColumnBase column)
    {
        IModel designTimeModel = this._designTimeModel.Model;

        return column.PropertyMappings
            .Select(mapping => mapping.Property)
            .Where(property => property.DeclaringType is IEntityType)
            .Select(property => designTimeModel.FindEntityType(property.DeclaringType.Name)?.FindProperty(property.Name)?.GetCollation())
            .FirstOrDefault(collation => collation is not null);
    }

    #endregion private methods
}
