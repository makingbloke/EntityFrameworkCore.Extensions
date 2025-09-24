// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Tag Name / Value Collection.
/// </summary>
internal sealed partial class TagCollection
{
    #region private fields

    /// <summary>
    /// Parameters dictionary.
    /// </summary>
    private readonly Dictionary<string, string> _tags = [];

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TagCollection"/> class.
    /// </summary>
    /// <param name="tags">A set of tags.</param>
    public TagCollection(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        Regex regex = SingleLineGetNameAndValueRegex();

        foreach (string tag in tags)
        {
            Match match = regex.Match(tag);

            if (match.Success)
            {
                string name = match.Groups["Name"].Value;
                string value = match.Groups["Value"].Value;
                this._tags[name] = value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagCollection"/> class.
    /// </summary>
    /// <param name="sql">A SQL string where the tags are stored in comments.</param>
    public TagCollection(string sql)
    {
        ArgumentException.ThrowIfNullOrEmpty(sql);

        Regex regex = MultilineSqlGetNameAndValueRegex();

        foreach (GroupCollection groups in regex.Matches(sql).Select(m => m.Groups))
        {
            string name = groups["Name"].Value;
            string value = groups["Value"].Value;
            this._tags[name] = value;
        }
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Tag a query with a name / value pair.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <param name="name">The tag name.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>The query with the tag added.</returns>
    public static IQueryable<TSource> TagQuery<TSource>(IQueryable<TSource> source, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(name);

        source = source.TagWith($"{name} {value}");
        return source;
    }

    /// <summary>
    /// Checks if an entry for the specified tag exists.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <returns><see langword="true"/> if the an entry exists else, <see langword="false"/>.</returns>
    public bool ContainsTag(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return this._tags.ContainsKey(name);
    }

    /// <summary>
    /// Tries to get the value for the specified tag.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="value">The value of the tag (out).</param>
    /// <returns><see langword="true"/> if an entry exists else, <see langword="false"/>.</returns>
    public bool TryGetValue(string name, out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return this._tags.TryGetValue(name, out value);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// A regex used to search a single line string for a name and a value separated by a space.
    /// </summary>
    [GeneratedRegex($"^(?<Name>{TagNames.TagNamePrefix}[^ ]+) (?<Value>.*)$", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex SingleLineGetNameAndValueRegex();

    /// <summary>
    /// A regex used to search a multi-line SQL string for a comment containing a name and a value separated by a space.
    /// </summary>
    [GeneratedRegex($@"^-- (?<Name>{TagNames.TagNamePrefix}[^ ]+) (?<Value>.*?)\r?$", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
    private static partial Regex MultilineSqlGetNameAndValueRegex();

    #endregion private methods
}
