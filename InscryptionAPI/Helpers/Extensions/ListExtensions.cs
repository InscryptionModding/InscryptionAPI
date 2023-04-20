using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

public static class ListExtensions
{
    public static T PopFirst<T>(this List<T> list)
    {
        T t = list[0];
        list.RemoveAt(0);
        return t;
    }

    public static T PopLast<T>(this List<T> list)
    {
        T t = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return t;
    }

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

    public static T GetRandom<T>(this List<T> list)
    {
        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static T GetSeededRandom<T>(this List<T> list, int seed)
    {
        int index = SeededRandom.Range(0, list.Count, seed);
        return list[index];
    }
}
