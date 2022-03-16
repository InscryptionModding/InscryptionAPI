using DiskCardGame;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods instead", true)]
public class EvolveIdentifier
{
    internal string name;
    internal int turnsToEvolve;
    internal CardModificationInfo mods;
    private EvolveParams evolution;

    public EvolveParams Evolution
    {
        get
        {
            if (this.evolution == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this.evolution;
        }
    }

    public EvolveIdentifier(string name, int turnsToEvolve, CardModificationInfo mods = null)
    {
        this.name = name;
        this.turnsToEvolve = turnsToEvolve;
        this.mods = mods;
    }

    private void SetParams(CardInfo card)
    {
        this.evolution = new EvolveParams();

        this.evolution.turnsToEvolve = this.turnsToEvolve;
        this.evolution.evolution = card;

        if (this.mods != null)
        {
            this.evolution.evolution.mods.Add(this.mods);
        }
    }

    public override string ToString()
    {
        return name;
    }
}