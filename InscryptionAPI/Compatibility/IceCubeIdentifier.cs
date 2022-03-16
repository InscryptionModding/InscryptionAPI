using DiskCardGame;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods instead", true)]
public class IceCubeIdentifier
{
    internal readonly string name;
    internal readonly CardModificationInfo mods;

    private IceCubeParams _iceCube;
    public IceCubeParams IceCube
    {
        get
        {
            if (this._iceCube == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this._iceCube;
        }
    }

    public IceCubeIdentifier(string name, CardModificationInfo mods = null)
    {
        this.name = name;
        this.mods = mods;
    }

    private void SetParams(CardInfo card)
    {
        this._iceCube = new IceCubeParams
        {
            creatureWithin = card
        };

        if (this.mods != null)
        {
            this._iceCube.creatureWithin.mods.Add(this.mods);
        }
    }

    public override string ToString()
    {
        return name;
    }
}
