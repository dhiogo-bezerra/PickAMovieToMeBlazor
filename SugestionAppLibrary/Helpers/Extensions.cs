using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugestionAppLibrary.Helpers;

public static class EnumerableHelper<E>
{
    private static Random r;

    static EnumerableHelper()
    {
        r = new Random();
    }

    public static T Random<T>(IEnumerable<T> input)
    {
        return input.ElementAt(r.Next(input.Count()));
    }

}

public static class EnumerableExtensions
{
    public static T Random<T>(this IEnumerable<T> input)
    {
        return EnumerableHelper<T>.Random(input);
    }
}

public static class ArrayExtensions
{
    /// <summary>
    /// Splits an array into several smaller arrays.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="size">The size of the smaller arrays.</param>
    /// <returns>An array containing smaller arrays.</returns>
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
        {
            yield return array.Skip(i * size).Take(size);
        }
    }
}