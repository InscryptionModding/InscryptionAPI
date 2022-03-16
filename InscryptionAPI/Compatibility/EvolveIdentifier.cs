using DiskCardGame;

namespace APIPlugin;

[Obsolete("Use CardManager and CardInfo extension methods instead", true)]
public class EvolveIdentifier
{
    internal readonly string name;
    internal readonly int turnsToEvolve;
    internal readonly CardModificationInfo mods;
    private EvolveParams _evolution;

    public EvolveParams Evolution
    {
        get
        {
            if (this._evolution == null)
                SetParams(CardLoader.GetCardByName(this.name));

            return this._evolution;
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
        this._evolution = new EvolveParams
        {
            turnsToEvolve = this.turnsToEvolve,
            evolution = card
        };

        if (this.mods != null)
        {
            this._evolution.evolution.mods.Add(this.mods);
        }
    }

    public override string ToString()
    {
        return name;
    }
}
