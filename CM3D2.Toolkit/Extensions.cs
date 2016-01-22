// --------------------------------------------------
// CM3D2.Toolkit - Extensions.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CM3D2.Toolkit
{
    internal static class Extensions
    {
        public static int CopyTo(this Stream input, Stream output)
        {
            const int BUFFER_SIZE = 16 * 1024; //16Kb Buffer
            var buffer = new byte[BUFFER_SIZE];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, bytesRead);

            return bytesRead;
        }

        public static bool SequenceEqual(this byte[] b1, byte[] b2)
        {
            if (b1 == b2)
                return true;
            if (b1 == null || b2 == null)
                return false;
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static IEnumerable<t> DistinctBy<t>(this IEnumerable<t> list, Func<t, object> propertySelector)
        {
            return list.GroupBy(propertySelector).Select(x => x.First());
        }

        public static IEnumerable<U> Flatten<T, U>(
            T element,
            Func<T, IEnumerable<T>> elementSelector,
            Func<T, IEnumerable<U>> valueSelector)
        {
            foreach (U node in valueSelector(element))
                yield return node;
            foreach (T subElem in elementSelector(element))
                foreach (U node in Flatten(subElem, elementSelector, valueSelector))
                    yield return node;
        }

        public static IEnumerable<U> Flatten<T, U>(T element, Func<T, T> elementSelector, Func<T, IEnumerable<U>> valueSelector)
        {
            foreach (U node in valueSelector(element))
                yield return node;
            foreach (U node in Flatten(elementSelector(element), elementSelector, valueSelector))
                yield return node;
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                   + "$";
        }
    }
}
