// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UUIDNext;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Utility Extensions.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Internal APIs used by various methods.")]
[SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Internal values used by various methods.")]
public static class UtilityExtensions
{
    #region private GetDbContext fields

    /// <summary>
    /// EF Core - Internal Query Compiler.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider)
        .GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo ContextFactoryField = typeof(QueryCompiler)
        .GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory Dependencies.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly PropertyInfo DependenciesProperty = typeof(RelationalQueryContextFactory)
        .GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance)!;

    #endregion private GetDbContext fields

    #region public CreateGuid methods

    /// <summary>
    /// Create a Guid that is suitable for use as a database key value.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns>A <see cref="Guid"/>.</returns>
    /// <remarks>
    /// SQL Server can store GUIDs but because of how they are handled date stamped GUIDs
    /// are not sorted in the correct order.
    /// This method uses a third party library (UUIDNext) to create GUIDS that are
    /// compatible with the supported database types.
    /// (The inbuilt EF Core <see cref="SequentialGuidValueGenerator"/>> class creates
    /// GUIDS for SQL Server that look correct but with have an incorrect version number).
    /// </remarks>
    public static Guid CreateGuid(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        string? databaseType = database.GetDatabaseType();

        Guid guid = databaseType switch
        {
            DatabaseTypes.Sqlite => Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite),
            DatabaseTypes.SqlServer => Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer),
            _ => throw new UnsupportedDatabaseTypeException(nameof(database)),
        };

        return guid;
    }

    #endregion public CreateGuid methods

    #region public DoesDatabaseExist methods

    /// <summary>
    /// Check if database exists.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see langword="bool"/> indicating if the database exists.</returns>
    public static async Task<bool> DoesDatabaseExistAsync(this DatabaseFacade database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        bool databaseExists = await database.GetService<IRelationalDatabaseCreator>().ExistsAsync(cancellationToken).ConfigureAwait(false);
        return databaseExists;
    }

    #endregion public DoesDatabaseExist methods

    #region public GetDbContext methods

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetDbContext(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        DbContext context = ((IDatabaseFacadeDependenciesAccessor)database).Context;
        return context;
    }

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetDbContext(this IQueryable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Provider is not EntityQueryProvider)
        {
            throw new ArgumentException("Query provider is not EF Core", nameof(source));
        }

        QueryCompiler queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(source.Provider)!;
        RelationalQueryContextFactory contextFactory = (RelationalQueryContextFactory)ContextFactoryField.GetValue(queryCompiler)!;
        QueryContextDependencies dependencies = (QueryContextDependencies)DependenciesProperty.GetValue(contextFactory)!;

        DbContext context = dependencies.CurrentContext.Context;
        return context;
    }

    #endregion public GetDbContext methods

    #region public UseCaseInsensitiveCollation methods

    /// <summary>
    /// Use Case Insensitive Collation.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <param name="databaseType">The database type.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static ModelBuilder UseCaseInsensitiveCollation(this ModelBuilder modelBuilder, string? databaseType)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                // SQLite does not support the concept of a global collation setting so set the NOCASE
                // collation on every text field in the entities: https://github.com/dotnet/efcore/issues/32051
                foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                                                        .SelectMany(t => t.GetProperties())
                                                        .Where(p => p.ClrType == typeof(string)))
                {
                    property.SetCollation("NOCASE");
                }

                break;

            case DatabaseTypes.SqlServer:
                modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(databaseType));
        }

        return modelBuilder;
    }

    #endregion public UseCaseInsensitiveCollation methods

    #region public GetDelimitedIdentifier methods

    /// <summary>
    /// Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <returns>The generated string.</returns>
    public static string GetDelimitedIdentifier(this DatabaseFacade database, string name)
    {
        ArgumentNullException.ThrowIfNull(database);
        NonRelationalDatabaseCheck(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return database.GetService<ISqlGenerationHelper>().DelimitIdentifier(name);
    }

    /// <summary>
    /// Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <returns>The generated string.</returns>
    public static string GetDelimitedIdentifier(this DatabaseFacade database, string name, string? schema)
    {
        ArgumentNullException.ThrowIfNull(database);
        NonRelationalDatabaseCheck(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string? normalizedSchema = string.IsNullOrWhiteSpace(schema) ? null : schema;

        return database.GetService<ISqlGenerationHelper>().DelimitIdentifier(name, normalizedSchema);
    }

    /// <summary>
    /// Appends the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append the generated string to.</param>
    /// <param name="name">The identifier to delimit.</param>
    public static void GetDelimitedIdentifier(this DatabaseFacade database, StringBuilder builder, string name)
    {
        ArgumentNullException.ThrowIfNull(database);
        NonRelationalDatabaseCheck(database);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        database.GetService<ISqlGenerationHelper>().DelimitIdentifier(builder, name);
    }

    /// <summary>
    /// Appends the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append the generated string to.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    public static void GetDelimitedIdentifier(this DatabaseFacade database, StringBuilder builder, string name, string? schema)
    {
        ArgumentNullException.ThrowIfNull(database);
        NonRelationalDatabaseCheck(database);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string? normalizedSchema = string.IsNullOrWhiteSpace(schema) ? null : schema;

        database.GetService<ISqlGenerationHelper>().DelimitIdentifier(builder, name, normalizedSchema);
    }

    #endregion public GetDelimitedIdentifier methods

    #region private methods

    /// <summary>
    /// Throws an exception if the database is not relational.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="database"/> corresponds. If you omit this parameter, the name of <paramref name="database"/> is used.</param>
    private static void NonRelationalDatabaseCheck(DatabaseFacade database, [CallerArgumentExpression(nameof(database))] string? paramName = default)
    {
        if (!database.IsRelational())
        {
            throw new ArgumentException($"The active database provider '{database.ProviderName}' is not a relational provider.", paramName);
        }
    }

    #endregion private methods
}
