// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Text Code Converter, which stores a <see cref="TextCode"/> as a string.
/// </summary>
public sealed class TextCodeConverter : ValueConverter<TextCode, string>
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TextCodeConverter"/> class.
    /// </summary>
    public TextCodeConverter()
        : base(code => code.Value, value => new TextCode(value))
    {
    }

    #endregion public constructors
}
