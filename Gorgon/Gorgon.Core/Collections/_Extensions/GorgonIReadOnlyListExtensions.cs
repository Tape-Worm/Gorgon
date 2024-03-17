using Gorgon.Properties;

namespace Gorgon.Collections;

/// <summary>
/// Extension methods for items that were inexplicably omitted from the <see cref="IReadOnlyList{T}"/> interface
/// </summary>
public static class GorgonIReadOnlyListExtensions
{
    /// <summary>
    /// Function to find the last index of an item using a predicate to filter through the list.
    /// </summary>
    /// <typeparam name="T">The type of values in the list.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="predicate">The predicate function used to evaluate the list items.</param>
    /// <returns>The index of the filtered item, or -1 if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/>, or the <paramref name="predicate"/> parameter is <b>null</b>.</exception>
    public static int LastIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(predicate);

        switch (list)
        {
            case List<T> concreteList:
                return concreteList.FindLastIndex(predicate);
        }

        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (predicate(list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to find the first index of an item using a predicate to filter through the list.
    /// </summary>
    /// <typeparam name="T">The type of values in the list.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="predicate">The predicate function used to evaluate the list items.</param>
    /// <returns>The index of the filtered item, or -1 if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/>, or the <paramref name="predicate"/> parameter is <b>null</b>.</exception>
    public static int FirstIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(predicate);

        switch (list)
        {
            case List<T> concreteList:
                return concreteList.FindIndex(predicate);
        }

        for (int i = 0; i < list.Count; ++i)
        {
            if (predicate(list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to determine if an item of type <typeparamref name="T"/> exists within the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="item">The item to find in the list.</param>
    /// <returns><b>true</b> if the <paramref name="item"/> was found, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method scans through a <see cref="IReadOnlyList{T}"/> to locate the specified <paramref name="item"/>. If the item is found, then <b>true</b> is returned, and if not, then <b>false</b> 
    /// is returned instead.
    /// </para>
    /// <para>
    /// The search will use a native implementation of the Contains method on the <paramref name="list"/> concrete type if available. Otherwise, if the type, <typeparamref name="T"/> implements 
    /// <see cref="IEquatable{T}"/> then that is used for comparing items the list. If that interface is not available, then <see cref="IComparable{T}"/> is used, and failing that, the 
    /// <see cref="object.Equals(object)"/> method is used to determine equality between the items in the list.
    /// </para>
    /// <para>
    /// For best performance, it is best to use a type that natively Contains in its concrete implementation.
    /// </para>
    /// </remarks>
    public static bool Contains<T>(this IReadOnlyList<T> list, T item)
    {
        switch (list)
        {
            case null:
                throw new ArgumentNullException(nameof(list));
            case T[] arrayList:
                // If the list is an array, use the built in functionality.
                return Array.IndexOf(arrayList, item) != -1;
            case IList<T> readWriteList:
                // If it implements IList<T>, then use the IndexOf on that.
                return readWriteList.Contains(item);
        }

        switch (item)
        {
            // If the items in the type implement IEquatable<T>, then this will suffice.
            case IEquatable<T> equalityItem:
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < list.Count; ++i)
                {
                    if (equalityItem.Equals(list[i]))
                    {
                        return true;
                    }
                }

                return false;
            // If no equality comparer is found, but we are comparable, then try to use that.
            case IComparable<T> comparerItem:
                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int i = 0; i < list.Count; ++i)
                {
                    if (comparerItem.CompareTo(list[i]) == 0)
                    {
                        return true;
                    }
                }

                return false;
        }

        // Finally, fall back to the object (and potentially boxing) method.
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < list.Count; ++i)
        {
            if (item.Equals(list[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Function to return the index of an item in a <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object in the collection.</typeparam>
    /// <param name="list">The list of items to evaluate.</param>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the <paramref name="item"/> in the <paramref name="list"/> if found, or -1 if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method scans through a <see cref="IReadOnlyList{T}"/> to locate the specified <paramref name="item"/>. If the item is found, the index of that item within the <paramref name="list"/> 
    /// is returned. Otherwise, if it is not found, <c>-1</c> is returned.
    /// </para>
    /// <para>
    /// The search will use a native implementation of the IndexOf method on the <paramref name="list"/> concrete type if available. Otherwise, if the type, <typeparamref name="T"/> implements 
    /// <see cref="IEquatable{T}"/> then that is used for comparing items the list. If that interface is not available, then <see cref="IComparable{T}"/> is used, and failing that, the 
    /// <see cref="object.Equals(object)"/> method is used to determine equality between the items in the list.
    /// </para>
    /// <para>
    /// For best performance, it is best to use a type that natively IndexOf in its concrete implementation.
    /// </para>
    /// </remarks>
    public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
    {
        switch (list)
        {
            case null:
                throw new ArgumentNullException(nameof(list));
            case T[] arrayList:
                // If the list is an array, use the built in functionality.
                return Array.IndexOf(arrayList, item);
            case List<T> readWriteList:
                // If it inherits List<T>, then use the IndexOf on that.
                return readWriteList.IndexOf(item);
            case IList<T> readWriteIList:
                // If it implements IList<T>, then use the IndexOf on that.
                return readWriteIList.IndexOf(item);
        }

        switch (item)
        {
            case IEquatable<T> equalityItem:
                // If the items in the type implement IEquatable<T>, then this will suffice.
                for (int i = 0; i < list.Count; ++i)
                {
                    if (equalityItem.Equals(list[i]))
                    {
                        return i;
                    }
                }

                return -1;
            case IComparable<T> comparerItem:
                // If no equality comparer is found, but we are comparable, then try to use that.
                for (int i = 0; i < list.Count; ++i)
                {
                    if (comparerItem.CompareTo(list[i]) == 0)
                    {
                        return i;
                    }
                }

                return -1;
        }

        // Finally, fall back to the object (and potentially boxing) method.
        for (int i = 0; i < list.Count; ++i)
        {
            if (item.Equals(list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to copy the contents of this read only list into an array.
    /// </summary>
    /// <typeparam name="T">The type of data to copy.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="array">The array that will receive the data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/>, or the <paramref name="array"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="array"/> isn't large enough to support the entire list.</exception>
    public static void CopyTo<T>(this IReadOnlyList<T> list, T[] array)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(array);

        if (array.Length < list.Count)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_ARRAY_TOO_SMALL, array.Length, list.Count), nameof(array));
        }

        if (list is List<T> concreteList)
        {
            concreteList.CopyTo(array);
            return;
        }

        if (list is Array arrayList)
        {
            Array.Copy(arrayList, array, list.Count);
            return;
        }

        for (int i = 0; i < list.Count; ++i)
        {
            array[i] = list[i];
        }
    }
}
