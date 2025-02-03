using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Message Table.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the Message Id.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Message Type.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the Message Data.
    /// </summary>
    [Required]
    [MaxLength(8192)]
    public required string Data { get; set; }

    /// <summary>
    /// Gets or sets the Message Lock Date.
    /// </summary>
    [Required]
    public DateTime LockDate { get; set; }
}
