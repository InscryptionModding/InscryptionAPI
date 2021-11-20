using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class TailIdentifier
	{
		private string name;
		private CardModificationInfo mods;
		private Texture2D tailLostTex;
		private TailParams tail;

		public TailParams Tail
		{
			get
			{
				if (this.tail == null)
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

				tail.tailLostPortrait = Sprite.Create(this.tailLostTex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
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
}
