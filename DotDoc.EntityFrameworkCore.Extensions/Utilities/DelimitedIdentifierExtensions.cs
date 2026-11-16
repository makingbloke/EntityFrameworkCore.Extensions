// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Delimited Identifier Extensions.
/// </summary>
public static class DelimitedIdentifierExtensions
{
    #region public methods

    /// <summary>
    /// Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <returns>The generated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The database provider is not relational, or <paramref name="name"/> is empty or white space.</exception>
    public static string GetDelimitedIdentifier(this DatabaseFacade database, string name)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ThrowIfNonRelationalDatabase(database);

        return database.GetService<ISqlGenerationHelper>().DelimitIdentifier(name);
    }

    /// <summary>
    /// Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <returns>The generated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The database provider is not relational, or <paramref name="name"/> is empty or white space.</exception>
    public static string GetDelimitedIdentifier(this DatabaseFacade database, string name, string? schema)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ThrowIfNonRelationalDatabase(database);

        string? normalizedSchema = string.IsNullOrWhiteSpace(schema) ? null : schema;

        return database.GetService<ISqlGenerationHelper>().DelimitIdentifier(name, normalizedSchema);
    }

    /// <summary>
    /// Appends the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append the generated string to.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <exception cref="ArgumentNullException"><paramref name="database"/>, <paramref name="builder"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The database provider is not relational, or <paramref name="name"/> is empty or white space.</exception>
    public static void GetDelimitedIdentifier(this DatabaseFacade database, StringBuilder builder, string name)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ThrowIfNonRelationalDatabase(database);

        database.GetService<ISqlGenerationHelper>().DelimitIdentifier(builder, name);
    }

    /// <summary>
    /// Appends the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="database">The database facade.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append the generated string to.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <exception cref="ArgumentNullException"><paramref name="database"/>, <paramref name="builder"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The database provider is not relational, or <paramref name="name"/> is empty or white space.</exception>
    public static void GetDelimitedIdentifier(this DatabaseFacade database, StringBuilder builder, string name, string? schema)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ThrowIfNonRelationalDatabase(database);

        string? normalizedSchema = string.IsNullOrWhiteSpace(schema) ? null : schema;

        database.GetService<ISqlGenerationHelper>().DelimitIdentifier(builder, name, normalizedSchema);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Throws an exception if the database is not relational.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="database"/> corresponds. If you omit this parameter, the name of <paramref name="database"/> is used.</param>
    /// <exception cref="ArgumentException">The database provider is not relational.</exception>
    private static void ThrowIfNonRelationalDatabase(DatabaseFacade database, [CallerArgumentExpression(nameof(database))] string? paramName = default)
    {
        if (!database.IsRelational())
        {
            throw new ArgumentException($"The active database provider '{database.ProviderName}' is not a relational provider.", paramName);
        }
    }

    #endregion private methods
}
