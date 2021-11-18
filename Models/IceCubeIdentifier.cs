using DiskCardGame;

namespace APIPlugin
{
	public class IceCubeIdentifier
	{
    private string name;
		private CardModificationInfo mods;
		private IceCubeParams iceCube;
		public IceCubeParams IceCube
		{
			get
			{
				if (this.iceCube == null)
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
			IceCubeParams iceCube = new IceCubeParams();

			iceCube.creatureWithin = card;

			if (this.mods != null)
			{
				iceCube.creatureWithin.mods.Add(this.mods);
			}

			this.iceCube = iceCube;
		}

		public override string ToString()
		{
			return name;
		}
	}
}
