// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotDoc.EntityFrameworkCore.Extensions.TableHints;

/// <summary>
/// Builds the table hints for a query, for each entity type's table (used by <see cref="TableHintExtensions.WithTableHints"/>).
/// </summary>
public sealed class TableHintsBuilder
{
    #region private fields

    /// <summary>
    /// The model the entity types are found in.
    /// </summary>
    private readonly IModel _model;

    /// <summary>
    /// The table hints for each table, in the order they were added, keyed by <see cref="CustomTagExtensions.GetTableKey(string, string?)"/>.
    /// </summary>
    private readonly List<KeyValuePair<string, ITableHint[]>> _tableHints = [];

    #endregion private fields

    #region internal constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TableHintsBuilder"/> class.
    /// </summary>
    /// <param name="model">The model the entity types are found in.</param>
    /// <exception cref="ArgumentNullException"><paramref name="model"/> is <see langword="null"/>.</exception>
    internal TableHintsBuilder(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        this._model = model;
    }

    #endregion internal constructors

    #region internal properties

    /// <summary>
    /// Gets the table hints for each table, in the order they were added, keyed by <see cref="CustomTagExtensions.GetTableKey(string, string?)"/>.
    /// </summary>
    internal IReadOnlyList<KeyValuePair<string, ITableHint[]>> TableHints => this._tableHints;

    #endregion internal properties

    #region public methods

    /// <summary>
    /// Adds table hints to the table (or view) of an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="tableHints">The table hints to add.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableHints"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="tableHints"/> is empty or contains a <see langword="null"/> hint, <typeparamref name="TEntity"/> isn't an entity type in the
    /// model (or is a shared type entity type), isn't mapped to a table or view, or its table has already been given hints.
    /// </exception>
    /// <remarks>
    /// The hints are added to every use of the table in the query, and only to that table. For an entity type in a table-per-type hierarchy that's
    /// the entity type's own table, not the tables of its base types.
    /// </remarks>
    public TableHintsBuilder For<TEntity>(params IEnumerable<ITableHint> tableHints)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(tableHints);

        IEntityType entityType = this._model.FindEntityType(typeof(TEntity))
            ?? throw new ArgumentException($"{typeof(TEntity).Name} is not an entity type in the model, or is a shared type entity type, so table hints can't be added to it.");

        return this.For(entityType, tableHints);
    }

    #endregion public methods

    #region internal methods

    /// <summary>
    /// Adds table hints to the table (or view) of an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="tableHints">The table hints to add.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType"/> or <paramref name="tableHints"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="tableHints"/> is empty or contains a <see langword="null"/> hint, the entity type isn't mapped to a table or view, or its
    /// table has already been given hints.
    /// </exception>
    /// <remarks>This overload takes the entity type itself, so it can be used for a shared type entity type (e.g. by the upsert methods).</remarks>
    internal TableHintsBuilder For(IEntityType entityType, params IEnumerable<ITableHint> tableHints)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(tableHints);

        ITableHint[] hints = [.. tableHints];

        if (hints.Length == 0)
        {
            throw new ArgumentException("At least one table hint must be supplied", nameof(tableHints));
        }

        if (hints.Any(th => th is null))
        {
            throw new ArgumentException("Null table hints values are not supported", nameof(tableHints));
        }

        // An entity type mapped to a view has no table name. NOEXPAND is only used on views.
        string? tableName = entityType.GetTableName();
        string? name = tableName ?? entityType.GetViewName();
        string? schema = tableName is not null ? entityType.GetSchema() : entityType.GetViewSchema();

        if (name is null)
        {
            throw new ArgumentException($"{entityType.DisplayName()} is not mapped to a table or view, so table hints can't be added to it.");
        }

        string tableKey = CustomTagExtensions.GetTableKey(name, schema);

        if (this._tableHints.Exists(th => th.Key == tableKey))
        {
            throw new ArgumentException($"Table hints have already been added to the table {tableKey} ({entityType.DisplayName()}).");
        }

        this._tableHints.Add(new(tableKey, hints));

        return this;
    }

    #endregion internal methods
}
