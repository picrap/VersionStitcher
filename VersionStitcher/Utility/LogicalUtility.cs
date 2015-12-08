// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Utility
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class LogicalUtility
    {
        /// <summary>
        /// Returns a || b, after both were evaluated.
        /// </summary>
        /// <param name="a">if set to <c>true</c> [a].</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <returns></returns>
        public static bool OrAny(this bool a, bool b)
        {
            return a || b;
        }

        public static bool AnyOfAll(this IEnumerable<bool> bools)
        {
            return bools.Aggregate(false, (p, t) => p || t);
        }
    }
}
