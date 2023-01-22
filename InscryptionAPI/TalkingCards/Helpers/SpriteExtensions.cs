using UnityEngine;

namespace InscryptionAPI.TalkingCards.Helpers;
internal static class SpriteExtensions
{
    internal static Sprite PivotBottom(this Sprite sprite)
    {
        if (sprite.pivot == AssetHelpers.PIVOT_BOTTOM) return sprite;
        Rect rect = sprite.rect;
        Texture2D tex = sprite.texture;
        return Sprite.Create(tex, rect, AssetHelpers.PIVOT_BOTTOM);
    }
}
