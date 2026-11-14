using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

/// <summary>
/// An Extensions Class for the <see cref="List{T}"/>.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Removes the First element of the passed <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> in which we want to remove the First item of.</param>
    /// <typeparam name="T">The type of <see cref="List{T}"/> we are operating on, for example <see cref="List{T}"/> where T is <see cref="string"/> would result in the Type being <see cref="string"/>.</typeparam>
    /// <returns>Returns the First item of <see cref="List{T}"/> after removing it from the <see cref="List{T}"/>.</returns>
    public static T PopFirst<T>(this List<T> list)
    {
        T t = list[0];
        list.RemoveAt(0);
        return t;
    }

    /// <summary>
    /// Removes the Last element of the passed <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> in which we want to remove the Last item of.</param>
    /// <typeparam name="T">The type of <see cref="List{T}"/> we are operating on, for example <see cref="List{T}"/> where T is <see cref="string"/> would result in the Type being <see cref="string"/>.</typeparam>
    /// <returns>Returns the Last item of <see cref="List{T}"/> after removing it from the <see cref="List{T}"/>.</returns>
    public static T PopLast<T>(this List<T> list)
    {
        T t = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return t;
    }

    /// <summary>
    /// Repeatedly adds the specified item to a <see cref="List{T}"/> X times.
    /// </summary>
    /// <param name="toRepeat">The Item in which we want to repeat.</param>
    /// <param name="times">The amount of times we want to repeat it.</param>
    /// <typeparam name="T">The type of <see cref="List{T}"/> we are operating on, for example <see cref="List{T}"/> where T is <see cref="string"/> would result in the Type being <see cref="string"/>.</typeparam>
    /// <returns>A new <see cref="List{T}"/> of the item specified by <see cref="toRepeat"/> repeated for the amount of times specified by the <see cref="times"/> parameter.</returns>
    public static List<T> Repeat<T>(this T toRepeat, int times)
    {
        List<T> repeated = new();
        if (toRepeat != null)
        {
            for (int i = 0; i < times; i++)
            {
                repeated.Add(toRepeat);
            }
        }
        return repeated;
    }

    /// <summary>
    /// A function used to get a Random item from the <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> we are using to get a Random Item from.</param>
    /// <typeparam name="T">The type of <see cref="List{T}"/> we are operating on, for example <see cref="List{T}"/> where T is <see cref="string"/> would result in the Type being <see cref="string"/>.</typeparam>
    /// <returns>A Random element from the passed <see cref="List{T}"/> using <see cref="UnityEngine.Random.Range(System.Int32, System.Int32)"/></returns>
    public static T GetRandom<T>(this List<T> list)
    {
        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    /// <summary>
    /// A function used to get a Seeded Random item from the <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> we are using to get a Seeded Random Item from.</param>
    /// <typeparam name="T">The type of <see cref="List{T}"/> we are operating on, for example <see cref="List{T}"/> where T is <see cref="string"/> would result in the Type being <see cref="string"/>.</typeparam>
    /// <returns>A Seeded Random element from the passed <see cref="List{T}"/> using <see cref="DiskCardGame.SeededRandom.Range(System.Int32, System.Int32, System.Int32)"/></returns>
    public static T GetSeededRandom<T>(this List<T> list, int seed)
    {
        int index = SeededRandom.Range(0, list.Count, seed);
        return list[index];
    }
}
