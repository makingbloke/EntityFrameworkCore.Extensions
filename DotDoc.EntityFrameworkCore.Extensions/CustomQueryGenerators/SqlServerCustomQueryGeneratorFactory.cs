// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// SQL Server Custom Query Generator Factory.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to replace query generator with a custom one.")]
internal sealed class SqlServerCustomQueryGeneratorFactory : SqlServerQuerySqlGeneratorFactory
{
    #region private members.

    /// <summary>
    /// Type mapping source.
    /// </summary>
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    /// SQL Server Singleton options.
    /// </summary>
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    #endregion private members

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerCustomQueryGeneratorFactory"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    /// <param name="typeMappingSource">Type mapping source.</param>
    /// <param name="sqlServerSingletonOptions">SQL Server Singleton options.</param>
    public SqlServerCustomQueryGeneratorFactory(QuerySqlGeneratorDependencies dependencies, IRelationalTypeMappingSource typeMappingSource, ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, typeMappingSource, sqlServerSingletonOptions)
    {
        this._typeMappingSource = typeMappingSource;
        this._sqlServerSingletonOptions = sqlServerSingletonOptions;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    public override QuerySqlGenerator Create()
    {
        return new SqlServerCustomQueryGenerator(this.Dependencies, this._typeMappingSource, this._sqlServerSingletonOptions);
    }

    #endregion public methods
}