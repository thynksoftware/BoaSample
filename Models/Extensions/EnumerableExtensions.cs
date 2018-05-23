using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Boa.Sample.Extentions
{
public static class EnumerableExtentions
    {
        public static bool IsEmpty(this IEnumerable collection)
        {
            return (collection == null || !collection.GetEnumerator().MoveNext());
        }

        public static int Count(this IEnumerable collection)
        {
            var count = 0;
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
            }
            return count;
        }


        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int maxLength)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            var enumerator = source.GetEnumerator();
            var batch = new List<T>(maxLength);

            while (enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);

                //Have we finished this batch? Yield it and start a new one.
                if (batch.Count != maxLength) continue;
                yield return batch;
                batch = new List<T>(maxLength);
            }

            //Yield the final batch if it has any elements
            if (batch.Count > 0)
            {
                yield return batch;
            }
            
            enumerator.Dispose();
        }

        /// <summary>Removes elements from an array if they match a predicate.</summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to search for elements matching the specified predicate. If this parameter is <c>null</c>, no action is taken and false is returned.</param>
        /// <param name="predicate">The predicate to check against.</param>
        /// <returns>true if any elements were removed from <paramref name="array"/>; otherwise, false.</returns>
        public static bool RemoveIf<T>(ref T[] array, Func<T, bool> predicate)
        {
            if (array == null)
            {
                return false;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var newLogicalEnd = 0;
            var last = array.Length;
            for (var first = 0; first != last; ++first)
            {
                if (!predicate(array[first]))
                {
                    array[newLogicalEnd++] = array[first];
                }
            }

            if (newLogicalEnd == last)
            {
                return false;
            }
            else
            {
                Array.Resize(ref array, newLogicalEnd);
                return true;
            }
        }

        /// <summary>
        /// Generates an enumerable collection in a random order.
        /// </summary>
        /// <typeparam name="T">The type stored in the enumerable collection.</typeparam>
        /// <param name="sequence">The enumerable containing the original data to scramble.</param>
        /// <returns>The sequence <paramref name="sequence"/> in a scrambled order.</returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence)
        {
            return sequence.Shuffle(new Random());
        }

        /// <summary>
        /// Generates an enumerable collection in a random order.
        /// </summary>
        /// <typeparam name="T">The type stored in the enumerable collection.</typeparam>
        /// <param name="sequence">The enumerable containing the original data to scramble.</param>
        /// <param name="randomNumberGenerator">The random number generator to use.</param>
        /// <returns>The sequence <paramref name="sequence"/> in a scrambled order.</returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence, Random randomNumberGenerator)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            if (randomNumberGenerator == null)
            {
                throw new ArgumentNullException(nameof(randomNumberGenerator));
            }

            // A naïve Knuth Fischer Yates shuffle.
            var values = sequence.ToList();
            var currentlySelecting = values.Count;
            while (currentlySelecting > 1)
            {
                // Next returns the next integer [0, currentlySelecting) which is why we need
                // to get the selected element before decrementing currentlySelecting
                // (To make it possible that the currentlySelecting is swapped with itself)
                var selectedElement = randomNumberGenerator.Next(currentlySelecting);
                --currentlySelecting;
                if (currentlySelecting == selectedElement) continue;
                var swapTemp = values[currentlySelecting];
                values[currentlySelecting] = values[selectedElement];
                values[selectedElement] = swapTemp;
            }

            return values;
        }

        /// <summary>
        /// Gets the types of the specified objects.
        /// </summary>
        /// <returns>The types.</returns>
        /// <param name="objects">The objects.</param>
        public static Type[] GetTypes(this IEnumerable objects)
        {
            var parametersType = new List<Type>();

            if (objects == null) return parametersType.ToArray();
            parametersType.AddRange(from object obj in objects select obj?.GetType());

            return parametersType.ToArray();
        }

        /// <summary>
        /// Iterates in the collection calling the action for each item and concatenating the result.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="self">The enumerable it self.</param>
        /// <param name="function">The ToString function.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string ToString<T>(this IEnumerable<T> self, Func<T, object> function)
        {
            var result = new StringBuilder();

            ForEach(self, i => result.Append(function(i)));

            return result.ToString();
        }

        /// <summary>
        /// Iterates in the collection calling the action for each item using String.Format.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="self">The enumerable it self.</param>
        /// <param name="format">The string format.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string ToString<T>(this IEnumerable<T> self, string format)
        {
            return self.ToString(i => String.Format(CultureInfo.InvariantCulture, format, i));
        }

        /// <summary>
        /// Iterates in the collection calling the action for each item.
        /// </summary>
        /// <param name="self">The enumerable it self.</param>
        /// <param name="action">The each action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the default equality comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the specified comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal.
        /// If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }

        /// <summary>
        /// Iterates in the collection calling the action for each item using index.
        /// </summary>
        /// <param name="self">The enumerable it self.</param>
        /// <param name="action">The each action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T, int> action)
        {
            var list = self.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }
    }
}