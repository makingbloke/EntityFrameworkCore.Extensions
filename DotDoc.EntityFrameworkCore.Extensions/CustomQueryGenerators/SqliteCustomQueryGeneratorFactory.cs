// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// SQLite Custom Query Generator Factory.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to replace query generator with a custom one.")]
internal sealed class SqliteCustomQueryGeneratorFactory : SqliteQuerySqlGeneratorFactory
{
    #region public constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteCustomQueryGeneratorFactory"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    public SqliteCustomQueryGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    #endregion public constructor

    #region public methods

    /// <inheritdoc/>
    public override QuerySqlGenerator Create()
    {
        return new SqliteCustomQueryGenerator(this.Dependencies);
    }

    #endregion public methods
}