// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Query Expression Interceptor.
/// </summary>
/// <remarks>
/// Rewrites each <c>FreeTextSearch</c> call that uses stemming into a non-stemming search of the stemming table, matched back to the row
/// by ROWID, so each call in a query can use its own <c>useStemming</c> value. The table a call searches isn't known until EF Core has
/// translated the query, so a candidate search is added for each entity type of the property's CLR type that has a stemming table,
/// and <see cref="SqliteFreeTextSearchTranslator"/> keeps the one for the column's table.
/// </remarks>
internal sealed class SqliteFreeTextSearchInterceptor : IQueryExpressionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteFreeTextSearchInterceptor Instance = new();

    /// <summary>
    /// The <see cref="StemmingSearch"/> method.
    /// </summary>
    internal static readonly MethodInfo StemmingSearchMethod = typeof(SqliteFreeTextSearchInterceptor).GetMethod(nameof(StemmingSearch))!;

    #endregion internal fields

    #region private fields

    /// <summary>
    /// The <see cref="EF.Property{TProperty}(object, string)"/> method.
    /// </summary>
    private static readonly MethodInfo EFPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!;

    #endregion private fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteFreeTextSearchInterceptor"/> class.
    /// </summary>
    private SqliteFreeTextSearchInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <summary>
    /// Marks a candidate stemming search: the search of one table's stemming table, used if the property is a column of that table.
    /// </summary>
    /// <param name="propertyReference">The property being searched.</param>
    /// <param name="tableName">The name of the table whose stemming table <paramref name="search"/> searches.</param>
    /// <param name="search">The search of the stemming table, matched back to the row by ROWID.</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    /// <exception cref="InvalidOperationException">The method is called directly, rather than translated to SQL as part of an EF Core query (e.g. because the query has switched to client evaluation).</exception>
    public static bool StemmingSearch(object propertyReference, [NotParameterized] string tableName, bool search) =>
        throw new InvalidOperationException(GetExceptionMessage());

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// The query searches with stemming, but no entity type of the property's CLR type has a stemming table set (<c>SetStemmingTable</c>),
    /// a stemming table is not in the model, the property isn't a property of an entity, or an entity type has no property mapped to its ROWID column.
    /// </exception>
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        return StemmingVisitor.Rewrite(eventData.Context!.Model, queryExpression);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get the method is not supported because the query has switched to client-evaluation message.
    /// </summary>
    /// <param name="methodName">The method name (auto populated).</param>
    /// <returns>The exception message.</returns>
    private static string GetExceptionMessage([CallerMemberName] string methodName = "") =>
        $"The '{methodName}' method is not supported because the query has switched to client-evaluation. This usually happens when the arguments to the method cannot be translated to server. Rewrite the query to avoid client evaluation of arguments so that method can be translated to server.";

    #endregion private methods

    #region private StemmingVisitor class

    /// <summary>
    /// Rewrites each <c>FreeTextSearch</c> call that uses stemming into candidate searches of the stemming tables.
    /// </summary>
    private sealed class StemmingVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// The names SQLite accepts for a table's ROWID column.
        /// </summary>
        private static readonly string[] RowIdColumnNames = ["rowid", "oid", "_rowid_"];

        /// <summary>
        /// The model.
        /// </summary>
        private readonly IModel _model;

        #endregion private fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StemmingVisitor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        private StemmingVisitor(IModel model)
        {
            this._model = model;
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Rewrites each <c>FreeTextSearch</c> call that uses stemming.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="expression">The query expression.</param>
        /// <returns>The rewritten query expression.</returns>
        /// <exception cref="InvalidOperationException">See <see cref="QueryCompilationStarting"/>.</exception>
        public static Expression Rewrite(IModel model, Expression expression)
        {
            StemmingVisitor visitor = new(model);
            return visitor.Visit(expression);
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.Contains(node.Method) && UsesStemming(node)
                ? this.RewriteStemmingSearch(node)
                : base.VisitMethodCall(node);
        }

        #endregion protected methods

        #region private methods

        /// <summary>
        /// Determines whether a <c>FreeTextSearch</c> call uses stemming.
        /// </summary>
        /// <param name="node">The call.</param>
        /// <returns><see langword="true"/> if its <c>useStemming</c> argument is <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        private static bool UsesStemming(MethodCallExpression node)
        {
            // useStemming is NotParameterized, so it's always a constant.
            return node.Method.GetParameters()
                .Zip(node.Arguments)
                .Any(pair => pair.First.Name == "useStemming" && pair.Second is ConstantExpression { Value: true });
        }

        /// <summary>
        /// Gets the entity the property reference is a property of, and a way to access the same property on another instance.
        /// </summary>
        /// <param name="propertyReference">The property reference argument.</param>
        /// <returns>The entity, and a function that accesses the property on another instance of the same type.</returns>
        /// <exception cref="InvalidOperationException">The property reference isn't a property of an entity.</exception>
        private static (Expression Entity, Func<Expression, Expression> PropertyOf) GetProperty(Expression propertyReference)
        {
            // The argument is an object, so the property is converted to object.
            Expression reference = propertyReference;

            while (reference is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
            {
                reference = unary.Operand;
            }

            return reference switch
            {
                MemberExpression { Expression: { } entity, Member: { } member } => (entity, instance => Expression.MakeMemberAccess(instance, member)),
                MethodCallExpression { Method: { Name: nameof(EF.Property) } method, Arguments: [{ } entity, { } name] } when method.DeclaringType == typeof(EF) =>
                    (entity, instance => Expression.Call(method, instance, name)),
                _ => throw new InvalidOperationException("FreeTextSearch with stemming needs a property of the entity being searched."),
            };
        }

        /// <summary>
        /// Gets the property mapped to an entity type's ROWID column.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The property.</returns>
        /// <exception cref="InvalidOperationException">No property of the entity type is mapped to ROWID.</exception>
        private static IProperty GetRowIdProperty(IEntityType entityType)
        {
            // Every FTS table has a ROWID, but EF Core can only refer to it through a property mapped to it (SQLite also accepts oid and _rowid_).
            return entityType.GetProperties().FirstOrDefault(property => property.GetTableColumnMappings().Any(mapping => RowIdColumnNames.Contains(mapping.Column.Name, StringComparer.OrdinalIgnoreCase)))
                ?? throw new InvalidOperationException($"FreeTextSearch with stemming needs a property of {entityType.DisplayName()} mapped to its ROWID column.");
        }

        /// <summary>
        /// Creates a call to <see cref="EF.Property{TProperty}(object, string)"/>.
        /// </summary>
        /// <param name="instance">The entity.</param>
        /// <param name="property">The property.</param>
        /// <returns>The call.</returns>
        private static MethodCallExpression PropertyCall(Expression instance, IProperty property)
        {
            return Expression.Call(EFPropertyMethod.MakeGenericMethod(property.ClrType), instance, Expression.Constant(property.Name));
        }

        /// <summary>
        /// Rewrites a <c>FreeTextSearch</c> call that uses stemming into a candidate search for each table that could be the one searched.
        /// </summary>
        /// <param name="node">The call.</param>
        /// <returns>The candidate searches, joined with OR.</returns>
        /// <exception cref="InvalidOperationException">See <see cref="QueryCompilationStarting"/>.</exception>
        private Expression RewriteStemmingSearch(MethodCallExpression node)
        {
            Expression propertyReference = node.Arguments[1];
            (Expression entity, Func<Expression, Expression> propertyOf) = GetProperty(propertyReference);

            // The main and stemming tables share a CLR type, so any entity type of that type with a stemming table could be the one searched.
            // The translator keeps the candidate for the column's table.
            List<IEntityType> candidates = [.. this._model.GetEntityTypes().Where(t => t.ClrType == entity.Type && !string.IsNullOrEmpty(t.GetStemmingTable()))];

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("Stemming table not specified.");
            }

            Expression? result = null;

            foreach (IEntityType candidate in candidates)
            {
                Expression marker = Expression.Call(
                    StemmingSearchMethod,
                    propertyReference,
                    Expression.Constant(candidate.GetTableName()),
                    this.CreateStemmingSearch(node, candidate, entity, propertyOf));

                result = result is null ? marker : Expression.OrElse(result, marker);
            }

            return result!;
        }

        /// <summary>
        /// Creates the search of a table's stemming table, matched back to the row by ROWID:
        /// <c>stemmingSet.Where(s => FreeTextSearch(s.Property, text, false)).Select(s => s.RowId).Contains(entity.RowId)</c>.
        /// </summary>
        /// <param name="node">The <c>FreeTextSearch</c> call.</param>
        /// <param name="entityType">The entity type of the table being searched.</param>
        /// <param name="entity">The entity being searched.</param>
        /// <param name="propertyOf">A function that accesses the searched property on another instance.</param>
        /// <returns>The search.</returns>
        /// <exception cref="InvalidOperationException">The stemming table is not in the model, or an entity type has no property mapped to ROWID.</exception>
        private MethodCallExpression CreateStemmingSearch(MethodCallExpression node, IEntityType entityType, Expression entity, Func<Expression, Expression> propertyOf)
        {
            string stemmingTableName = entityType.GetStemmingTable()!;

            IEntityType stemmingEntityType = this._model.GetEntityTypes().FirstOrDefault(t => t.ClrType == entityType.ClrType && t.GetTableName() == stemmingTableName)
                ?? throw new InvalidOperationException($"Stemming table {stemmingTableName} not found.");

            IProperty rowId = GetRowIdProperty(entityType);
            IProperty stemmingRowId = GetRowIdProperty(stemmingEntityType);
            ParameterExpression stemmingEntity = Expression.Parameter(entityType.ClrType, "s");

            // The two ROWID properties could be mapped to different CLR types (e.g. long and int), so match them as the stemming table's type.
            Expression entityRowId = rowId.ClrType == stemmingRowId.ClrType
                ? PropertyCall(entity, rowId)
                : Expression.Convert(PropertyCall(entity, rowId), stemmingRowId.ClrType);

            // The same FreeTextSearch call, without stemming, on the stemming table's property.
            ParameterInfo[] parameters = node.Method.GetParameters();
            Expression[] searchArguments = [.. node.Arguments.Select((argument, index) => parameters[index].Name switch
            {
                "propertyReference" => Expression.Convert(propertyOf(stemmingEntity), argument.Type),
                "useStemming" => Expression.Constant(false),
                _ => argument,
            })];

            MethodCallExpression where = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Where),
                [entityType.ClrType],
                new EntityQueryRootExpression(stemmingEntityType),
                Expression.Quote(Expression.Lambda(Expression.Call(node.Method, searchArguments), stemmingEntity)));

            MethodCallExpression select = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                [entityType.ClrType, stemmingRowId.ClrType],
                where,
                Expression.Quote(Expression.Lambda(PropertyCall(stemmingEntity, stemmingRowId), stemmingEntity)));

            return Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Contains),
                [stemmingRowId.ClrType],
                select,
                entityRowId);
        }

        #endregion private methods
    }

    #endregion private StemmingVisitor class
}
