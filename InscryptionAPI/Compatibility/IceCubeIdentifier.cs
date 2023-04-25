using DiskCardGame;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods instead", true)]
public class IceCubeIdentifier
{
    internal string name;
    internal CardModificationInfo mods;

    private IceCubeParams iceCube;
    public IceCubeParams IceCube
    {
        get
        {
            if (this.iceCube == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this.iceCube;
        }
    }

    public IceCubeIdentifier(string name, CardModificationInfo mods = null)
    {
        this.name = name;
        this.mods = mods;
    }

    private void SetParams(CardInfo card)
    {
        this.iceCube = new IceCubeParams();

        this.iceCube.creatureWithin = card;

        if (this.mods != null)
        {
            this.iceCube.creatureWithin.mods.Add(this.mods);
        }
    }

    public override string ToString()
    {
        return name;
    }
}