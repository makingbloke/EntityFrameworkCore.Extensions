// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Shared helpers for the Execute Update tests.
/// </summary>
internal static class ExecuteUpdateTestUtils
{
    #region internal methods

    /// <summary>
    /// Add <see cref="Customer"/> to the model, with its <see cref="Customer.Address"/> mapped as an owned type or a complex type.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="owned"><see langword="true"/> to map the address as an owned type (<c>OwnsOne</c>), <see langword="false"/> for a complex type (<c>ComplexProperty</c>).</param>
    internal static void AddCustomer(ModelBuilder modelBuilder, bool owned)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable(nameof(Customer));

            if (owned)
            {
                builder.OwnsOne(e => e.Address);
            }
            else
            {
                builder.ComplexProperty(e => e.Address);
            }
        });
    }

    /// <summary>
    /// Add <see cref="MappedEntity"/>, <see cref="Animal"/> and <see cref="Dog"/> to the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <remarks>
    /// <see cref="MappedEntity.Name"/> is mapped to the <c>name_col</c> column, <see cref="MappedEntity.Day"/> is stored as a string, and a query filter
    /// hides soft deleted rows. <see cref="Animal"/> and <see cref="Dog"/> use table-per-hierarchy inheritance.
    /// </remarks>
    internal static void AddMappedEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MappedEntity>(builder =>
        {
            builder.ToTable(nameof(MappedEntity));
            builder.Property(e => e.Name).HasColumnName("name_col");
            builder.Property(e => e.Day).HasConversion<string>().HasMaxLength(16);
            builder.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Animal>().ToTable(nameof(Animal));
        modelBuilder.Entity<Dog>();
    }

    /// <summary>
    /// Add <see cref="Animal"/> and <see cref="Dog"/> to the model with table-per-type or table-per-concrete-type inheritance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="tablePerConcreteType"><see langword="true"/> for table-per-concrete-type (<c>UseTpcMappingStrategy</c>), <see langword="false"/> for table-per-type (<c>UseTptMappingStrategy</c>).</param>
    internal static void AddAnimals(ModelBuilder modelBuilder, bool tablePerConcreteType)
    {
        modelBuilder.Entity<Animal>(builder =>
        {
            if (tablePerConcreteType)
            {
                builder.UseTpcMappingStrategy();
            }
            else
            {
                builder.UseTptMappingStrategy();
            }

            builder.ToTable(nameof(Animal));
        });

        modelBuilder.Entity<Dog>().ToTable(nameof(Dog));
    }

    /// <summary>
    /// Assert that a context's <see cref="ExecuteUpdateState"/> holds no call (as it should once a call has finished).
    /// </summary>
    /// <param name="context">The context.</param>
    internal static void AssertStateCleared(DbContext context)
    {
        ExecuteUpdateState state = context.GetService<ExecuteUpdateState>();

        Assert.AreEqual(QueryType.Unknown, state.QueryType, "The query type has not been cleared.");
        Assert.IsNull(state.Sql, "The SQL has not been cleared.");
        Assert.IsNull(state.Parameters, "The parameters have not been cleared.");
    }

    /// <summary>
    /// Assert that an exception is the one thrown when a get rows method is called on <see cref="TestTable1"/> without the
    /// RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="databaseType">Database type.</param>
    /// <param name="methodName">The name of the method that requires the clause.</param>
    internal static void AssertReturningClauseNotUsedException(NotSupportedException exception, string databaseType, string methodName)
    {
        string expectedMessage = databaseType switch
        {
            DatabaseTypes.Sqlite => $"{methodName} requires the RETURNING clause, but the table {nameof(TestTable1)} is configured with UseSqlReturningClause(false).",
            DatabaseTypes.SqlServer => $"{methodName} requires the OUTPUT clause, but the table {nameof(TestTable1)} is configured with UseSqlOutputClause(false).",
            _ => throw new UnsupportedDatabaseTypeException()
        };

        Assert.AreEqual(expectedMessage, exception.Message, "Unexpected exception message");
    }

    /// <summary>
    /// Turn off the RETURNING (SQLite) / OUTPUT (SQL Server) clause for <see cref="TestTable1"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="databaseType">Database type.</param>
    internal static void TurnOffReturningClause(ModelBuilder modelBuilder, string databaseType)
    {
        modelBuilder.Entity<TestTable1>()
            .ToTable(tableBuilder =>
            {
                switch (databaseType)
                {
                    case DatabaseTypes.Sqlite:
                        tableBuilder.UseSqlReturningClause(false);
                        break;

                    case DatabaseTypes.SqlServer:
                        tableBuilder.UseSqlOutputClause(false);
                        break;

                    default:
                        throw new UnsupportedDatabaseTypeException();
                }
            });
    }

    #endregion internal methods
}
