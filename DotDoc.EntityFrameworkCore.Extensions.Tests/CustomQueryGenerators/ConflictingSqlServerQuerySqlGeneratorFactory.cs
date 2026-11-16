// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CustomQueryGenerators;

/// <summary>
/// Conflicting SQL Server Query SQL Generator Factory.
/// Stands in for another extension library that replaces <see cref="IQuerySqlGeneratorFactory"/>, deriving from EF Core's factory as the library's own factory does.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Simulates another extension library replacing the query generator.")]
internal sealed class ConflictingSqlServerQuerySqlGeneratorFactory : SqlServerQuerySqlGeneratorFactory
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictingSqlServerQuerySqlGeneratorFactory"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    /// <param name="typeMappingSource">Type mapping source.</param>
    /// <param name="sqlServerSingletonOptions">SQL Server Singleton options.</param>
    public ConflictingSqlServerQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies, IRelationalTypeMappingSource typeMappingSource, ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, typeMappingSource, sqlServerSingletonOptions)
    {
    }

    #endregion public constructors
}
