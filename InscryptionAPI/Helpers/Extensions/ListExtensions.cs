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
}
