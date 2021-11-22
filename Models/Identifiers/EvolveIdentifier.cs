using DiskCardGame;

namespace APIPlugin
{
	public class EvolveIdentifier
	{
		private string name;
		private int turnsToEvolve;
		private CardModificationInfo mods;
		private EvolveParams evolution;

		public EvolveParams Evolution
		{
			get
			{
				if (this.evolution == null)
				{
					if (NewCard.cards.Exists((CardInfo x) => x.name == this.name))
					{
						SetParams(NewCard.cards.Find((CardInfo x) => x.name == this.name));
					}
					else
					{
						if (ScriptableObjectLoader<CardInfo>.AllData.Exists((CardInfo x) => x.name == this.name))
						{
							SetParams(ScriptableObjectLoader<CardInfo>.AllData.Find((CardInfo x) => x.name == this.name));
						}
					}
				}

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
			EvolveParams _evolution = new EvolveParams();

			_evolution.turnsToEvolve = this.turnsToEvolve;

			_evolution.evolution = card;

			if (this.mods != null)
			{
				_evolution.evolution.mods.Add(this.mods);
			}

			this.evolution = _evolution;
		}

		public override string ToString()
		{
			return name;
		}
	}
}