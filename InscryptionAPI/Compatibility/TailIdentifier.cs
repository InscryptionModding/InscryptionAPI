using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods", true)]
public class TailIdentifier
{
    internal readonly string name;
    internal readonly CardModificationInfo mods;
    internal readonly Texture2D tailLostTex;
    private TailParams _tail;

    public TailParams Tail
    {
        get
        {
            if (this._tail == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this._tail;
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
        TailParams iTail = new TailParams();

        if (this.tailLostTex is not null)
        {
            this.tailLostTex.name = this.name;
            this.tailLostTex.filterMode = FilterMode.Point;

            iTail.tailLostPortrait = this.tailLostTex.ConvertTexture(TextureHelper.SpriteType.CardPortrait);
            iTail.tailLostPortrait.name = this.name;
        }

        iTail.tail = card;

        if (this.mods != null)
        {
            iTail.tail.mods.Add(this.mods);
        }

        this._tail = iTail;
    }

    public override string ToString()
    {
        return name;
    }
}
