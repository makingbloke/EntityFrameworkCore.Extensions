// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Runtime.Serialization;
using DotDoc.EntityFrameworkCore.Extensions.Model;

namespace DotDoc.EntityFrameworkCore.Extensions.Exceptions;

/// <summary>
/// Unique constraint exception.
/// </summary>
[Serializable]
public class UniqueConstraintException : Exception
{
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
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="details">Unique constraint details <see cref="UniqueConstraintDetails"/>.</param>
    public UniqueConstraintException(string message, Exception innerException, UniqueConstraintDetails details)
        : base(message, innerException)
    {
        this.Details = details;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    /// <param name="info">The serialization info <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The streaming context <see cref="StreamingContext"/>.</param>
    protected UniqueConstraintException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// gets the Unique constraint details.
    /// </summary>
    public UniqueConstraintDetails Details { get; }
}