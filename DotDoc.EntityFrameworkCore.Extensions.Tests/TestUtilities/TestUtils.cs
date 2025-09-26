// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Reflection;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;

/// <summary>
/// Test Utilities.
/// </summary>
public static class TestUtils
{
    #region public methods

    /// <summary>
    /// Create a display name for a test method that uses the <see cref="DynamicDataAttribute"/>.
    /// </summary>
    /// <param name="methodInfo">The test method <see cref="MethodInfo"/>.</param>
    /// <param name="data">The data (parameters passed to the test method).</param>
    /// <returns>A string containing a formatted comma separated list of the values.</returns>
    public static string CreateDynamicDisplayName(MethodInfo methodInfo, object?[] data)
    {
        StringBuilder displayName = new();

        ParameterInfo[] parameters = methodInfo.GetParameters();

        int parameterCount = Math.Min(parameters.Length, data.Length);

        for (int i = 0; i < parameterCount; i++)
        {
            if (i > 0)
            {
                displayName.Append(", ");
            }

            displayName.Append(parameters[i].Name);
            displayName.Append(": ");

            string displayValue = data[i] switch
            {
                null => "null",
                string s => FormatStringForDisplay(s),
                FormattableString fs => FormatStringForDisplay(FormattableString.Invariant(fs)),
                _ => $"{data[i]}"
            };

            displayName.Append(displayValue);
        }

        return displayName.ToString();
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Format a <see cref="string"/> for display.
    /// </summary>
    /// <param name="value">The string to be formatted.</param>
    /// <returns>The string truncated to 15 characters and surrounded with double quotes.</returns>
    private static string FormatStringForDisplay(string value)
    {
        // Add Ellipsis if we truncate the string.
        const int MaxStringLength = 15;
        const char Ellipsis = '\u2026';

        string displayValue = $@"""{(value.Length > MaxStringLength ? value[..MaxStringLength] + Ellipsis : value)}""";
        return displayValue;
    }

    #endregion private methods
}
