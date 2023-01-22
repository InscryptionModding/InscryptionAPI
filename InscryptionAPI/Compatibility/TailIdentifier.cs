using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods", true)]
public class TailIdentifier
{
    internal string name;
    internal CardModificationInfo mods;
    internal Texture2D tailLostTex;
    private TailParams tail;

    public TailParams Tail
    {
        get
        {
            if (this.tail == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this.tail;
        }
    }

    public TailIdentifier(string name, Texture2D tailLostTex = null, CardModificationInfo mods = null)
    {
        this.name = name;
        this.mods = mods;
        this.tailLostTex = tailLostTex;
    }

    private void SetParams(CardInfo card)
    {
        TailParams tail = new TailParams();

        if (this.tailLostTex is not null)
        {
            this.tailLostTex.name = this.name;
            this.tailLostTex.filterMode = FilterMode.Point;

            tail.tailLostPortrait = TextureHelper.ConvertTexture(this.tailLostTex, TextureHelper.SpriteType.CardPortrait);
            tail.tailLostPortrait.name = this.name;
        }

        tail.tail = card;

        if (this.mods != null)
        {
            tail.tail.mods.Add(this.mods);
        }

        this.tail = tail;
    }

    public override string ToString()
    {
        return name;
    }
}