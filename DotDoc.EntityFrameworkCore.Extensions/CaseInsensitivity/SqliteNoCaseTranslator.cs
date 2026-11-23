// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Translator.
/// </summary>
/// <remarks>
/// Translates string methods on a SQLite column with the NOCASE collation, or on <c>EF.Functions.Collate(..., "NOCASE")</c>, to the nocase_*
/// functions registered by <see cref="SqliteNoCaseReplacementInterceptor"/>, so they ignore case as the collation does.
/// The methods that return a string (Replace, Substring, ToLower, ToUpper and the Trim methods) give their result the NOCASE collation,
/// as SQL Server gives a function's result its argument's collation.
/// </remarks>
internal sealed class SqliteNoCaseTranslator : IMethodCallTranslator
{
    #region private fields

    /// <summary>
    /// The translation of each method the translator handles, keyed by the method, with where to find the column whose collation decides it.
    /// </summary>
    private static readonly FrozenDictionary<MethodInfo, MethodTranslation> Translations = new Dictionary<MethodInfo, MethodTranslation>
    {
        [GetMethod(typeof(string), nameof(string.Contains), [typeof(string)])] = new(null, static (translator, column, arguments) => translator.TranslateContains(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.Contains), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateContains(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.EndsWith), [typeof(string)])] = new(null, static (translator, column, arguments) => translator.TranslateEndsWith(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.EndsWith), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateEndsWith(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.IndexOf), [typeof(string)])] = new(null, static (translator, column, arguments) => translator.TranslateIndexOf(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.IndexOf), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateIndexOf(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.IndexOf), [typeof(string), typeof(int)])] = new(null, static (translator, column, arguments) => translator.TranslateIndexOfWithStartIndex(column, arguments[0], arguments[1])),
        [GetMethod(typeof(string), nameof(string.IndexOf), [typeof(char), typeof(int)])] = new(null, static (translator, column, arguments) => translator.TranslateIndexOfWithStartIndex(column, arguments[0], arguments[1])),
        [GetMethod(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])] = new(1, static (translator, _, arguments) => translator.TranslateLike(arguments)),
        [GetMethod(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])] = new(1, static (translator, _, arguments) => translator.TranslateLike(arguments)),
        [GetMethod(typeof(string), nameof(string.Replace), [typeof(string), typeof(string)])] = new(null, static (translator, column, arguments) => translator.TranslateReplace(column, arguments[0], arguments[1])),
        [GetMethod(typeof(string), nameof(string.Replace), [typeof(char), typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateReplace(column, arguments[0], arguments[1])),
        [GetMethod(typeof(string), nameof(string.StartsWith), [typeof(string)])] = new(null, static (translator, column, arguments) => translator.TranslateStartsWith(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.StartsWith), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateStartsWith(column, arguments[0])),
        [GetMethod(typeof(string), nameof(string.Substring), [typeof(int)])] = new(null, static (translator, column, arguments) => translator.TranslateSubstring(column, arguments[0], null)),
        [GetMethod(typeof(string), nameof(string.Substring), [typeof(int), typeof(int)])] = new(null, static (translator, column, arguments) => translator.TranslateSubstring(column, arguments[0], arguments[1])),
        [GetMethod(typeof(string), nameof(string.ToLower), [])] = new(null, static (translator, column, _) => translator.TranslateStringFunction(SqliteUnicodeFunctionNames.LowerFunction, column)),
        [GetMethod(typeof(string), nameof(string.ToUpper), [])] = new(null, static (translator, column, _) => translator.TranslateStringFunction(SqliteUnicodeFunctionNames.UpperFunction, column)),
        [GetMethod(typeof(string), nameof(string.TrimEnd), [])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.RTrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.TrimEnd), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.RTrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.TrimEnd), [typeof(char[])])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.RTrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.Trim), [])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.TrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.Trim), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.TrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.Trim), [typeof(char[])])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.TrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.TrimStart), [])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.LTrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.TrimStart), [typeof(char)])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.LTrimFunction, column, arguments)),
        [GetMethod(typeof(string), nameof(string.TrimStart), [typeof(char[])])] = new(null, static (translator, column, arguments) => translator.TranslateTrim(SqliteUnicodeFunctionNames.LTrimFunction, column, arguments)),
    }.ToFrozenDictionary();

    /// <summary>
    /// The SQL expression factory.
    /// </summary>
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// The design-time model, which has the collations the runtime model leaves out.
    /// </summary>
    private readonly IDesignTimeModel _designTimeModel;

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
        // The marker added by SqliteNoCaseQueryExpressionInterceptor must always be translated (it isn't a real function), so its argument is
        // returned unchanged unless it's a NOCASE expression without the collation already.
        if (method == SqliteNoCaseQueryExpressionInterceptor.KeepCollationMethod)
        {
            SqlExpression expression = arguments[0];

            return expression is not CollateExpression && this.IsNoCase(expression)
                ? WithNoCaseCollation(expression)
                : expression;
        }

        if (Translations.TryGetValue(method, out MethodTranslation entry))
        {
            // Get the column whose collation decides the translation, either the argument at ColumnIndex (the match expression for EF.Functions.Like),
            // or the instance the method is called on.
            SqlExpression? column = entry.ColumnIndex.HasValue
                ? arguments[entry.ColumnIndex.Value]
                : instance;

            if (column is not null && this.IsNoCase(column))
            {
                return entry.Translate(this, column, arguments);
            }
        }

        return null;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the method specified.
    /// </summary>
    /// <param name="classType">The type of the class that holds the method (passed as a parameter rather than a generic so we can look in static classes too).</param>
    /// <param name="name">The method name.</param>
    /// <param name="argumentTypes">The types of the method's arguments.</param>
    /// <returns>The method.</returns>
    private static MethodInfo GetMethod(Type classType, string name, Type[] argumentTypes)
    {
        return classType.GetMethod(name, argumentTypes)
            ?? throw new InvalidOperationException($"Method: {classType.Name}.{name}({string.Join(", ", argumentTypes.Select(type => type.Name))}) not found");
    }

    /// <summary>
    /// Gives the string result of a nocase_* function the NOCASE collation.
    /// </summary>
    /// <param name="function">The function call.</param>
    /// <returns><c>function COLLATE NOCASE</c>.</returns>
    /// <remarks>
    /// A SQLite function's result has no collation, so it compares as BINARY, but on SQL Server a function's result has its argument's collation.
    /// The functions are only used on a NOCASE argument, so comparing the result, or calling another string method on it, ignores case as on SQL Server.
    /// </remarks>
    private static CollateExpression WithNoCaseCollation(SqlExpression function)
    {
        return new CollateExpression(function, SqliteUnicodeFunctionNames.NoCaseCollation);
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
    /// <remarks>
    /// <c>nocase_endswith(x, s)</c>. EF Core's own translation can't be kept: for a constant or parameter it uses LIKE, which ignores the collation
    /// and only ignores the case of ASCII letters; for a column argument it compares <c>substr(x, ..., length(s))</c> with <c>s</c>, which uses the collation
    /// of <c>s</c>, and can cut the suffix in the wrong place when the match is a different length (é against e + a combining accent).
    /// nocase_endswith uses the culture-aware IsSuffix, which handles both and needs no LIKE escaping.
    /// </remarks>
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
    /// <c>(nocase_instr(substr(x, start + 1), s) - 1) + start</c>, the same shape as EF Core's own translation with instr, so it gives the same results
    /// for every start index, including <c>start - 1</c> when there's no match. substr is SQLite's own.
    /// </remarks>
    private SqlExpression TranslateIndexOfWithStartIndex(SqlExpression instance, SqlExpression search, SqlExpression startIndex)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, search);

        SqlExpression substring = this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.SubstrFunction,
            [
                this._sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping),
                this._sqlExpressionFactory.Add(startIndex, this._sqlExpressionFactory.Constant(1)),
            ],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(string),
            typeMapping);

        return this._sqlExpressionFactory.Add(
            this._sqlExpressionFactory.Subtract(this.NoCaseInstr(substring, search), this._sqlExpressionFactory.Constant(1)),
            startIndex);
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
    /// <remarks>
    /// <c>nocase_replace(x, a, b) COLLATE NOCASE</c>, the same shape as EF Core's own translation with replace, with the NOCASE collation
    /// (see <see cref="WithNoCaseCollation"/>).
    /// </remarks>
    private CollateExpression TranslateReplace(SqlExpression instance, SqlExpression oldValue, SqlExpression newValue)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, oldValue, newValue);

        SqlExpression function = this._sqlExpressionFactory.Function(
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

        return WithNoCaseCollation(function);
    }

    /// <summary>
    /// Translates <see cref="string.StartsWith(string)"/> or <see cref="string.StartsWith(char)"/>.
    /// </summary>
    /// <param name="instance">The string to check.</param>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The translation.</returns>
    /// <remarks>
    /// <c>nocase_startswith(x, s)</c>. EF Core's own translation can't be kept: for a constant or parameter it uses LIKE, which ignores the collation
    /// and only ignores the case of ASCII letters; for a column argument it compares <c>substr(x, 1, length(s))</c> with <c>s</c>, which uses the collation
    /// of <c>s</c>, and can cut the prefix in the wrong place when the match is a different length (é against e + a combining accent).
    /// nocase_startswith uses the culture-aware IsPrefix, which handles both and needs no LIKE escaping.
    /// </remarks>
    private SqlExpression TranslateStartsWith(SqlExpression instance, SqlExpression prefix)
    {
        return this.BoolFunction(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, instance, prefix);
    }

    /// <summary>
    /// Translates <see cref="string.ToLower()"/> or <see cref="string.ToUpper()"/>, which EF Core translates to a SQLite function of the value alone.
    /// </summary>
    /// <param name="functionName">The SQLite function: lower or upper (the Unicode replacements).</param>
    /// <param name="instance">The string.</param>
    /// <returns>The translation.</returns>
    /// <remarks>
    /// <c>function(x) COLLATE NOCASE</c>, EF Core's own translation with the NOCASE collation (see <see cref="WithNoCaseCollation"/>).
    /// Case conversion doesn't depend on the collation, so the functions are the lower and upper replacements, as in EF Core's translation.
    /// </remarks>
    private CollateExpression TranslateStringFunction(string functionName, SqlExpression instance)
    {
        SqlExpression function = this._sqlExpressionFactory.Function(
            functionName,
            [instance],
            nullable: true,
            argumentsPropagateNullability: [true],
            typeof(string),
            instance.TypeMapping);

        return WithNoCaseCollation(function);
    }

    /// <summary>
    /// Translates <see cref="string.Substring(int)"/> or <see cref="string.Substring(int, int)"/>.
    /// </summary>
    /// <param name="instance">The string.</param>
    /// <param name="startIndex">The zero-based start position.</param>
    /// <param name="length">The length, or <see langword="null"/> for the rest of the string.</param>
    /// <returns>The translation.</returns>
    /// <remarks>
    /// <c>substr(x, start + 1[, length]) COLLATE NOCASE</c>, EF Core's own translation with the NOCASE collation (see <see cref="WithNoCaseCollation"/>).
    /// substr is SQLite's own.
    /// </remarks>
    private CollateExpression TranslateSubstring(SqlExpression instance, SqlExpression startIndex, SqlExpression? length)
    {
        List<SqlExpression> functionArguments = [instance, this._sqlExpressionFactory.Add(startIndex, this._sqlExpressionFactory.Constant(1))];

        if (length is not null)
        {
            functionArguments.Add(length);
        }

        SqlExpression function = this._sqlExpressionFactory.Function(
            SqliteUnicodeFunctionNames.SubstrFunction,
            functionArguments,
            nullable: true,
            argumentsPropagateNullability: functionArguments.Select(_ => true),
            typeof(string),
            instance.TypeMapping);

        return WithNoCaseCollation(function);
    }

    /// <summary>
    /// Translates <see cref="string.Trim()"/>, <see cref="string.TrimStart()"/> or <see cref="string.TrimEnd()"/>, or one of their overloads
    /// that take a <see cref="char"/> or a <see cref="char"/> array.
    /// </summary>
    /// <param name="functionName">SQLite's function: trim, ltrim or rtrim. With the characters, the nocase_* version of it is used.</param>
    /// <param name="instance">The string to trim.</param>
    /// <param name="arguments">The method's arguments: none, or the characters to trim.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    /// <remarks>
    /// <c>trim(x) COLLATE NOCASE</c> with no characters, or <c>nocase_trim(x, 'chars') COLLATE NOCASE</c> with the characters as one string constant
    /// (or ltrim / nocase_ltrim, rtrim / nocase_rtrim): the same shape as EF Core's own translation, with the NOCASE collation (see <see cref="WithNoCaseCollation"/>).
    /// </remarks>
    private CollateExpression? TranslateTrim(string functionName, SqlExpression instance, IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> functionArguments = [instance];

        // With no characters it trims spaces, which have no case, so SQLite's own function is used, as in EF Core's translation.
        // With characters, the nocase_* function is the built-in's name with the nocase_ prefix (as SqliteUnicodeFunctionNames defines them).
        if (arguments.Count > 0)
        {
            string? characters = arguments[0] switch
            {
                SqlConstantExpression { Value: char value } => new string(value, 1),
                SqlConstantExpression { Value: char[] { Length: > 0 } value } => new string(value),
                _ => null,
            };

            if (characters is null)
            {
                return null;
            }

            functionName = SqliteUnicodeFunctionNames.NoCasePrefix + functionName;
            functionArguments.Add(this._sqlExpressionFactory.Constant(characters, instance.TypeMapping));
        }

        SqlExpression function = this._sqlExpressionFactory.Function(
            functionName,
            functionArguments,
            nullable: true,
            argumentsPropagateNullability: functionArguments.Select(_ => true),
            typeof(string),
            instance.TypeMapping);

        return WithNoCaseCollation(function);
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
    /// <see langword="true"/> if <paramref name="expression"/> is <c>EF.Functions.Collate(..., "NOCASE")</c>, a table column with the NOCASE collation,
    /// or a concatenation, <c>??</c> or conditional expression whose operands with a known collation are all NOCASE, otherwise <see langword="false"/>
    /// (including when the collation isn't known).
    /// </returns>
    private bool IsNoCase(SqlExpression expression)
    {
        return string.Equals(this.GetCollation(expression), SqliteUnicodeFunctionNames.NoCaseCollation, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the collation of an expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>The collation, or <see langword="null"/> if it isn't known.</returns>
    private string? GetCollation(SqlExpression expression)
    {
        // A concatenation (||), ?? (COALESCE) or conditional (CASE) takes the collation of its operands, as on SQL Server.
        return expression switch
        {
            CollateExpression collateExpression => collateExpression.Collation,
            ColumnExpression { Column: { } column } => this.GetCollation(column),
            SqlBinaryExpression { OperatorType: ExpressionType.Add } binary when binary.Type == typeof(string) => this.GetCombinedCollation([binary.Left, binary.Right]),
            SqlFunctionExpression { Name: "COALESCE", IsBuiltIn: true, Arguments: { } coalesceArguments } => this.GetCombinedCollation(coalesceArguments),
            CaseExpression caseExpression => this.GetCombinedCollation([.. caseExpression.WhenClauses.Select(clause => clause.Result), caseExpression.ElseResult]),
            _ => null,
        };
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
        // We don't use Linq here to save on memory allocation.
        IModel designTimeModel = this._designTimeModel.Model;
        IReadOnlyList<IColumnMappingBase> mappings = column.PropertyMappings;

        for (int index = 0; index < mappings.Count; index++)
        {
            IProperty property = mappings[index].Property;

            if (property.DeclaringType is IEntityType)
            {
                string? collation = designTimeModel.FindEntityType(property.DeclaringType.Name)?.FindProperty(property.Name)?.GetCollation();

                if (collation is not null)
                {
                    return collation;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the collation of a concatenation, <c>??</c> or conditional expression from its operands.
    /// </summary>
    /// <param name="operands">The operands.</param>
    /// <returns>
    /// The collation of the operands with a known collation, or <see langword="null"/> if none has one, or if they have different collations.
    /// </returns>
    private string? GetCombinedCollation(IEnumerable<SqlExpression?> operands)
    {
        // Operands with no known collation (constants, parameters, columns without one) don't count. Different collations are left to SQLite's
        // own rules (SQL Server raises a collation conflict error there).
        string? result = null;

        foreach (SqlExpression? operand in operands)
        {
            string? collation = operand is null ? null : this.GetCollation(operand);

            if (collation is not null)
            {
                if (result is null)
                {
                    result = collation;
                }
                else if (!string.Equals(result, collation, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }
        }

        return result;
    }

    #endregion private methods

    #region private MethodTranslation struct

    /// <summary>
    /// The translation of a method, and where to find the column whose collation decides it.
    /// </summary>
    /// <param name="ColumnIndex">The index of the argument that holds the column, or <see langword="null"/> if the column is the instance the method is called on.</param>
    /// <param name="Translate">
    /// The translation, which takes the translator, the column (already checked to have the NOCASE collation) and the method's arguments,
    /// and returns the SQL, or <see langword="null"/> to use EF Core's own translation.
    /// </param>
    private readonly record struct MethodTranslation(int? ColumnIndex, Func<SqliteNoCaseTranslator, SqlExpression, IReadOnlyList<SqlExpression>, SqlExpression?> Translate);

    #endregion private MethodTranslation struct
}
