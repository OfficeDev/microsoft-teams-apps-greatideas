// <copyright file="RegexExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers.Extensions
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class to provide regular expression extension methods.
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// Escaping unsafe and reserved characters from Azure Search Service search query.
        /// Special characters that requires escaping includes
        /// + - &amp; | ! ( ) { } [ ] ^ " ~ * ? : \ /
        /// Refer https://docs.microsoft.com/en-us/azure/search/query-lucene-syntax#escaping-special-characters to know more.
        /// </summary>
        /// <param name="query">Query which the user had typed in search field.</param>
        /// <returns>Returns string escaping unsafe and reserved characters.</returns>
        public static string EscapeCharactersInQuery(this string query)
        {
            string pattern = @"([_|\\@&\?\*\+!-:~'\^/(){}<>#&\[\]])";
            string substitution = "\\$&";
            return Regex.Replace(query, pattern, substitution);
        }
    }
}
