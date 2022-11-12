// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Runtime.Serialization;
using DotDoc.EntityFrameworkCore.Extensions.Model;

namespace DotDoc.EntityFrameworkCore.Extensions.Exceptions;

/// <summary>
/// Unique Constraint Exception.
/// </summary>
[Serializable]
public class UniqueConstraintException : Exception
{
    /// <summary>
    /// Unique constraint exception message.
    /// </summary>
    private const string ExceptionMessage = "Unique constraint exception";

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintException"/> class.
    /// </summary>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="details">Unique constraint details <see cref="UniqueConstraintDetails"/>.</param>
    public UniqueConstraintException(Exception innerException, UniqueConstraintDetails details)
        : base(ExceptionMessage, innerException)
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
    /// Gets the Unique constraint details.
    /// </summary>
    public UniqueConstraintDetails Details { get; }
}