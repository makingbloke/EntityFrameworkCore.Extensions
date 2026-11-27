// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Text Code, a strongly typed value stored as a string by a value converter (<see cref="TextCodeConverter"/>).
/// </summary>
/// <param name="Value">The value.</param>
public sealed record TextCode(string Value);
