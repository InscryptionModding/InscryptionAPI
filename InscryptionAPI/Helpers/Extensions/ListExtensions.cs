using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

public static class ListExtensions
{
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
    
    public static T GetSeededRandom<T>(this List<T> list, int seed)
    {
        int index = SeededRandom.Range(0, list.Count, seed);
        return list[index];
    }
}
