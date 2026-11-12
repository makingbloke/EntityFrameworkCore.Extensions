// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// Describes a quoted token in a SQL dialect, e.g. 'string', "identifier" or [identifier].
/// </summary>
/// <param name="Open">The opening quote character.</param>
/// <param name="Close">The closing quote character.</param>
/// <param name="Type">The type of token the quotes create.</param>
/// <param name="DoubledCloseIsEscape">A value indicating whether two closing quotes together are an escaped quote (e.g. 'it''s' or [a]]b]).</param>
internal sealed record SqlQuote(char Open, char Close, SqlTokenType Type, bool DoubledCloseIsEscape);
