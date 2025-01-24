// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Model;

namespace DotDoc.EntityFrameworkCore.Extensions.Exceptions;

/// <summary>
/// Unique Constraint Exception.
/// </summary>
public sealed class UniqueConstraintException : Exception
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    public UniqueConstraintException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    public UniqueConstraintException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    public UniqueConstraintException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="details">Unique constraint details <see cref="UniqueConstraintDetails"/>.</param>
    public UniqueConstraintException(Exception innerException, UniqueConstraintDetails details)
        : base(null, innerException)
    {
        ArgumentNullException.ThrowIfNull(details);

        this.Details = details;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the Unique constraint details.
    /// </summary>
    public UniqueConstraintDetails? Details { get; }

    #endregion public properties
}