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
/// <item><description>Contains and IndexOf use nocase_instr.</description></item>
/// <item><description>StartsWith and EndsWith use nocase_startswith and nocase_endswith (EF Core uses LIKE, which needs the prefix or suffix escaped).</description></item>
/// <item><description>EF.Functions.Like uses nocase_like, when its match expression has the NOCASE collation.</description></item>
/// <item><description>Replace uses nocase_replace.</description></item>
/// <item><description>Trim, TrimStart and TrimEnd with constant characters use nocase_trim, nocase_ltrim and nocase_rtrim.</description></item>
/// <item><description>ToLower and ToUpper don't depend on a collation, so they always use nocase_lower and nocase_upper.</description></item>
/// </list>
/// Apart from StartsWith, EndsWith and IndexOf(string, int) (which uses the three-argument nocase_instr rather than substr), each translation has the same shape
/// as EF Core's own SQLite translation, with the nocase_* function in place of the built-in one.
/// The collation is read from the design-time model, as the runtime model has no collations (<see cref="RelationalPropertyExtensions.GetCollation(IReadOnlyProperty)"/>
/// throws on a runtime property). EF Core includes the model in its compiled query cache key, so a translation is cached per model.
/// </remarks>
internal sealed class SqlNoCaseTranslator : IMethodCallTranslator
{
    #region private constants

    /// <summary>
    /// The name of the NOCASE collation (SQLite collation names are case insensitive).
    /// </summary>
    private const string NoCaseCollationName = "NOCASE";

    /// <summary>
    /// The name of the nocase_endswith function.
    /// </summary>
    private const string NoCaseEndsWithFunctionName = "nocase_endswith";

    /// <summary>
    /// The name of the nocase_instr function.
    /// </summary>
    private const string NoCaseInstrFunctionName = "nocase_instr";

    /// <summary>
    /// The name of the nocase_like function.
    /// </summary>
    private const string NoCaseLikeFunctionName = "nocase_like";

    /// <summary>
    /// The name of the nocase_lower function.
    /// </summary>
    private const string NoCaseLowerFunctionName = "nocase_lower";

    /// <summary>
    /// The name of the nocase_ltrim function.
    /// </summary>
    private const string NoCaseLTrimFunctionName = "nocase_ltrim";

    /// <summary>
    /// The name of the nocase_replace function.
    /// </summary>
    private const string NoCaseReplaceFunctionName = "nocase_replace";

    /// <summary>
    /// The name of the nocase_rtrim function.
    /// </summary>
    private const string NoCaseRTrimFunctionName = "nocase_rtrim";

    /// <summary>
    /// The name of the nocase_startswith function.
    /// </summary>
    private const string NoCaseStartsWithFunctionName = "nocase_startswith";

    /// <summary>
    /// The name of the nocase_trim function.
    /// </summary>
    private const string NoCaseTrimFunctionName = "nocase_trim";

    /// <summary>
    /// The name of the nocase_upper function.
    /// </summary>
    private const string NoCaseUpperFunctionName = "nocase_upper";

    #endregion private constants

    #region private fields

    /// <summary>
    /// The <see cref="string.Contains(string)"/> and <see cref="string.Contains(char)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] ContainsMethods =
    [
        GetStringMethod(nameof(string.Contains), typeof(string)),
        GetStringMethod(nameof(string.Contains), typeof(char)),
    ];

    /// <summary>
    /// The <see cref="string.EndsWith(string)"/> and <see cref="string.EndsWith(char)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] EndsWithMethods =
    [
        GetStringMethod(nameof(string.EndsWith), typeof(string)),
        GetStringMethod(nameof(string.EndsWith), typeof(char)),
    ];

    /// <summary>
    /// The <see cref="string.IndexOf(string)"/> and <see cref="string.IndexOf(char)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] IndexOfMethods =
    [
        GetStringMethod(nameof(string.IndexOf), typeof(string)),
        GetStringMethod(nameof(string.IndexOf), typeof(char)),
    ];

    /// <summary>
    /// The <see cref="string.IndexOf(string, int)"/> and <see cref="string.IndexOf(char, int)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] IndexOfWithStartIndexMethods =
    [
        GetStringMethod(nameof(string.IndexOf), typeof(string), typeof(int)),
        GetStringMethod(nameof(string.IndexOf), typeof(char), typeof(int)),
    ];

    /// <summary>
    /// The <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string)"/> and
    /// <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string, string)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] LikeMethods =
    [
        typeof(DbFunctionsExtensions).GetRuntimeMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])!,
        typeof(DbFunctionsExtensions).GetRuntimeMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!,
    ];

    /// <summary>
    /// The <see cref="string.Replace(string, string)"/> and <see cref="string.Replace(char, char)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] ReplaceMethods =
    [
        GetStringMethod(nameof(string.Replace), typeof(string), typeof(string)),
        GetStringMethod(nameof(string.Replace), typeof(char), typeof(char)),
    ];

    /// <summary>
    /// The <see cref="string.StartsWith(string)"/> and <see cref="string.StartsWith(char)"/> methods.
    /// </summary>
    private static readonly MethodInfo[] StartsWithMethods =
    [
        GetStringMethod(nameof(string.StartsWith), typeof(string)),
        GetStringMethod(nameof(string.StartsWith), typeof(char)),
    ];

    /// <summary>
    /// The <see cref="string.ToLower()"/> method.
    /// </summary>
    private static readonly MethodInfo ToLowerMethod = GetStringMethod(nameof(string.ToLower));

    /// <summary>
    /// The <see cref="string.ToUpper()"/> method.
    /// </summary>
    private static readonly MethodInfo ToUpperMethod = GetStringMethod(nameof(string.ToUpper));

    /// <summary>
    /// The Trim, TrimStart and TrimEnd methods that take characters, and the nocase_* function each one is translated to.
    /// </summary>
    private static readonly Dictionary<MethodInfo, string> TrimMethods = new()
    {
        [GetStringMethod(nameof(string.Trim), typeof(char))] = NoCaseTrimFunctionName,
        [GetStringMethod(nameof(string.Trim), typeof(char[]))] = NoCaseTrimFunctionName,
        [GetStringMethod(nameof(string.TrimStart), typeof(char))] = NoCaseLTrimFunctionName,
        [GetStringMethod(nameof(string.TrimStart), typeof(char[]))] = NoCaseLTrimFunctionName,
        [GetStringMethod(nameof(string.TrimEnd), typeof(char))] = NoCaseRTrimFunctionName,
        [GetStringMethod(nameof(string.TrimEnd), typeof(char[]))] = NoCaseRTrimFunctionName,
    };

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
    /// Initializes a new instance of the <see cref="SqlNoCaseTranslator"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <param name="designTimeModel">The design-time model.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> or <paramref name="designTimeModel"/> is <see langword="null"/>.</exception>
    public SqlNoCaseTranslator(ISqlExpressionFactory sqlExpressionFactory, IDesignTimeModel designTimeModel)
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

        if (instance is not null && method.DeclaringType == typeof(string))
        {
            return this.TranslateStringMethod(instance, method, arguments);
        }

        if (LikeMethods.Contains(method))
        {
            return this.TranslateLike(arguments);
        }

        return null;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Gets a public instance method of <see cref="string"/>.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameterTypes">The types of the method's parameters.</param>
    /// <returns>The method.</returns>
    private static MethodInfo GetStringMethod(string name, params Type[] parameterTypes)
    {
        return typeof(string).GetRuntimeMethod(name, parameterTypes)!;
    }

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
    /// Translates a method of <see cref="string"/>.
    /// </summary>
    /// <param name="instance">The string the method is called on.</param>
    /// <param name="method">The method.</param>
    /// <param name="arguments">The method's arguments.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation.</returns>
    private SqlExpression? TranslateStringMethod(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        // ToLower and ToUpper don't depend on a collation.
        if (method.Equals(ToLowerMethod) || method.Equals(ToUpperMethod))
        {
            string functionName = method.Equals(ToLowerMethod) ? NoCaseLowerFunctionName : NoCaseUpperFunctionName;
            return this._sqlExpressionFactory.Function(functionName, [instance], nullable: true, argumentsPropagateNullability: [true], typeof(string), instance.TypeMapping);
        }

        if (!this.IsNoCase(instance))
        {
            return null;
        }

        // instr(x, s) > 0, as EF Core.
        if (ContainsMethods.Contains(method))
        {
            return this._sqlExpressionFactory.GreaterThan(this.NoCaseInstr(instance, arguments[0]), this._sqlExpressionFactory.Constant(0));
        }

        // instr(x, s) - 1, as EF Core.
        if (IndexOfMethods.Contains(method))
        {
            return this._sqlExpressionFactory.Subtract(this.NoCaseInstr(instance, arguments[0]), this._sqlExpressionFactory.Constant(1));
        }

        if (IndexOfWithStartIndexMethods.Contains(method))
        {
            return this.TranslateIndexOfWithStartIndex(instance, arguments[0], arguments[1]);
        }

        if (StartsWithMethods.Contains(method) || EndsWithMethods.Contains(method))
        {
            string functionName = StartsWithMethods.Contains(method) ? NoCaseStartsWithFunctionName : NoCaseEndsWithFunctionName;
            return this.BoolFunction(functionName, instance, arguments[0]);
        }

        if (ReplaceMethods.Contains(method))
        {
            return this.TranslateReplace(instance, arguments[0], arguments[1]);
        }

        if (TrimMethods.TryGetValue(method, out string? trimFunctionName))
        {
            return this.TranslateTrim(trimFunctionName, instance, arguments[0]);
        }

        return null;
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
            NoCaseInstrFunctionName,
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
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the match expression doesn't have the NOCASE collation.</returns>
    /// <remarks>nocase_like takes the pattern first, as SQLite's like function does (<c>x LIKE p</c> is <c>like(p, x)</c>).</remarks>
    private SqlExpression? TranslateLike(IReadOnlyList<SqlExpression> arguments)
    {
        SqlExpression match = arguments[1];

        if (!this.IsNoCase(match))
        {
            return null;
        }

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
            NoCaseLikeFunctionName,
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
            NoCaseReplaceFunctionName,
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
    /// Translates a Trim, TrimStart or TrimEnd method that takes characters.
    /// </summary>
    /// <param name="functionName">The nocase_* function to use.</param>
    /// <param name="instance">The string to trim.</param>
    /// <param name="argument">The characters to trim.</param>
    /// <returns>The translation, or <see langword="null"/> to use EF Core's own translation if the characters aren't constant.</returns>
    private SqlExpression? TranslateTrim(string functionName, SqlExpression instance, SqlExpression argument)
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
    /// Creates a call to nocase_instr.
    /// </summary>
    /// <param name="instance">The string to search.</param>
    /// <param name="search">The string or character to find.</param>
    /// <returns>The call.</returns>
    private SqlExpression NoCaseInstr(SqlExpression instance, SqlExpression search)
    {
        RelationalTypeMapping? typeMapping = ExpressionExtensions.InferTypeMapping(instance, search);

        return this._sqlExpressionFactory.Function(
            NoCaseInstrFunctionName,
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

        return string.Equals(collation, NoCaseCollationName, StringComparison.OrdinalIgnoreCase);
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
